import { Component, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import ParseHelper from 'src/app/shared/utils/parse.helper';
import { HttpClient } from '@angular/common/http';
import { ActivityService } from 'src/app/feature/specific/activity/activity.service';
import { IULink } from 'src/app/shared/interfaces/general.interface';

@Component({
  selector: 'event-create-page',
  templateUrl: './event-create-page.html',
  styleUrls: ['./event-create-page.less'],
  encapsulation: ViewEncapsulation.None
})
export class EventCreatePage {
  data: any;
  parsedData: any;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private activityService: ActivityService,
  ) {
    this.route.data.subscribe(data => {
      if (!data.requiresRedirect.get()) {
        this.data = data;
        this.parsedData = ParseHelper.parseUbaselineData(this.data);
        this.parsedData.data.members = Object.values(this.parsedData.data.members);
        this.parsedData.data.availableTags = Object.values(this.parsedData.data.tags);
      } else {
        this.router.navigate([data.errorLink.get().originalUrl.get()]);
      }
    });
  }

  onSubmit(data) {
    this.activityService.createEvent(data).subscribe((res: IULink) => {
      this.router.navigate([res.originalUrl]);
    })
  }

  onCancel() {
    this.router.navigate([this.parsedData.data.links.feed.originalUrl]);
  }
}