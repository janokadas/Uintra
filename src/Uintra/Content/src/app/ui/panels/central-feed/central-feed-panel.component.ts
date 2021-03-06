import { Component, ViewEncapsulation, OnInit, NgZone, OnDestroy, HostListener } from '@angular/core';
import { PublicationsService } from './helpers/publications.service';
import { SignalrService } from 'src/app/shared/services/general/signalr.service';
import { TranslateService } from '@ngx-translate/core';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { Subscription } from 'rxjs';
import { IFilterState, ICentralFeedPanel, IPublicationsResponse } from 'src/app/shared/interfaces/panels/central-feed/central-feed-panel.interface';
import {PopUpComponent} from "../../../shared/ui-elements/pop-up/pop-up.component";
import { MqService } from 'src/app/shared/services/general/mq.service';

@Component({
  selector: 'central-feed-panel',
  templateUrl: './central-feed-panel.html',
  styleUrls: ['./central-feed-panel.less'],
  encapsulation: ViewEncapsulation.None
})
export class CentralFeedPanel implements OnInit, OnDestroy {
  @HostListener("window:resize", ["$event"])
  getScreenSize(event?) {
    this.deviceWidth = window.innerWidth;
    if (this.mq.smDown(this.deviceWidth)) {
      this.isScrollDisabled = true;
      this.showLoadMore = true;
    } else {
      this.isScrollDisabled = false;
      this.showLoadMore = false;
    }
  }

  private $publications: Subscription;
  private deviceWidth: number;

  public data: ICentralFeedPanel;
  public tabs: Array<any> = null;
  public selectTabFilters: Array<IFilterState>;
  public selectedTabType: number;
  public feed: Array<any> = [];
  public currentPage = 1;
  public isFeedLoading = false;
  public isResponseFailed = false;
  public isScrollDisabled = false;
  public showLoadMore = false;

  constructor(
    private publicationsService: PublicationsService,
    private signalrService: SignalrService,
    private ngZone: NgZone,
    private translate: TranslateService,
    private mq: MqService,
  ) { }

  public ngOnInit(): void {
    this.deviceWidth = window.innerWidth;
    this.tabs = this.filtersBuilder();
    this.signalrService.getReloadFeedSubjects().subscribe(s => {
      this.reloadFeed();
    });
    if (this.mq.smDown(this.deviceWidth)) {
      this.onLoadMore();
    }
  }

  public index = (index, item): string => {
    return item.id;
  }

  public ngOnDestroy(): void {
    if (this.$publications) { this.$publications.unsubscribe(); }
  }

  public filtersBuilder() {
    const filtersFromServer = this.data.tabs;
    const allOption = {
      type: "0",
      isActive: true,
      links: null,
      title: this.translate.instant('centralFeed.Filter.All.lnk'),
      filters: [
        {
          key: 'ShowPinned',
          title: this.translate.instant('centralFeedList.ShowPinned.chkbx'),
          isActive: false
        }
      ]
    };

    filtersFromServer.unshift(allOption);

    return filtersFromServer;
  }

  public reloadFeed(): void {
    if (typeof window !== 'undefined') { window.scrollTo(0, 0); }
    this.resetFeed();
    this.getPublications();
  }

  public getPublications(): void {
    const FilterState = {};

    this.selectTabFilters.forEach(filter => {
      FilterState[filter.key] = !!filter.isActive;
    });
    const data = {
      TypeId: this.selectedTabType,
      FilterState,
      Page: this.currentPage,
      groupId: this.data.groupId
    };
    this.isFeedLoading = true;
    this.$publications = this.publicationsService
      .getPublications(data)
      .pipe(finalize(() => this.isFeedLoading = false))
      .subscribe(
        (next: IPublicationsResponse) => {
          this.isScrollDisabled = next.feed.length === 0;
          this.showLoadMore = false;
          this.concatWithCurrentFeed(next.feed);
          this.isResponseFailed = false;
        },
        (error: HttpErrorResponse) => this.isResponseFailed = true);
  }

  public concatWithCurrentFeed(data): void {
    this.ngZone.run(() => {
      this.feed = this.feed.concat(data);
    });
  }

  public onLoadMore(): void {
    this.currentPage += 1;
    this.getPublications();
  }

  public onScroll(): void {
    this.onLoadMore();
  }

  public resetFeed(): void {
    this.feed = [];
    this.currentPage = 1;
  }

  public selectFilters({ selectedTabType, selectTabFilters }): void {
    this.selectTabFilters = selectTabFilters;
    this.selectedTabType = selectedTabType;
    this.reloadFeed();
  }
}
