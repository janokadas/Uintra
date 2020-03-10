import { Component, ViewEncapsulation } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AddButtonService } from '../../../main-layout/left-navigation/components/my-links/add-button.service';
import ParseHelper from 'src/app/shared/utils/parse.helper';

@Component({
  selector: 'uintra-my-groups-page',
  templateUrl: './uintra-my-groups-page.html',
  styleUrls: ['./uintra-my-groups-page.less'],
  encapsulation: ViewEncapsulation.None
})
export class UintraMyGroupsPage {
  data: any;

  constructor(
    private route: ActivatedRoute,
    private addButtonService: AddButtonService

  ) {
    this.route.data.subscribe(data => {
      this.data = ParseHelper.parseUbaselineData(data);
      this.addButtonService.setPageId(data.id.get());
    });
  }
}
