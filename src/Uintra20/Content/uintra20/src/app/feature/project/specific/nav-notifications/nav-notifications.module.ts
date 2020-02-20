import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NavNotificationsComponent } from './nav-notifications.component';
import { HttpClientModule } from '@angular/common/http';
import { UserAvatarModule } from '../../reusable/ui-elements/user-avatar/user-avatar.module';
import { RouterModule } from '@angular/router';
import { NotificationsItemComponent } from './notifications-item/notifications-item.component';
import { ClickOutsideModule } from '../../reusable/directives/click-outside/click-outside.module';
import { UlinkModule } from 'src/app/services/pipes/link/ulink.module';
import { NotificationCountComponent } from './notification-count/notification-count.component';
import { PinActivityModule } from '../activity/pin-activity/pin-activity.module';

@NgModule({
  declarations: [NavNotificationsComponent, NotificationsItemComponent, NotificationCountComponent],
  imports: [
    CommonModule,
    HttpClientModule,
    UserAvatarModule,
    RouterModule,
    ClickOutsideModule,
    UlinkModule,
    PinActivityModule
  ],
  exports: [ NavNotificationsComponent, NotificationsItemComponent ]
})
export class NavNotificationsModule { }