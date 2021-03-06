import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GroupsWrapperComponent } from './groups-wrapper.component';
import { RouterModule } from '@angular/router';
import { UlinkModule } from 'src/app/shared/pipes/link/ulink.module';
import { BreadcrumbsModule } from 'src/app/shared/ui-elements/breadcrumbs/breadcrumbs.module';



@NgModule({
  declarations: [GroupsWrapperComponent],
  imports: [
    CommonModule,
    RouterModule,
    UlinkModule,
    BreadcrumbsModule,
  ],
  exports: [GroupsWrapperComponent]
})
export class GroupsWrapperModule { }
