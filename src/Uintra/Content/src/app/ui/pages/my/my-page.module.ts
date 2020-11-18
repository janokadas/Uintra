import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { UbaselineCoreModule } from 'ubaseline-next-for-uintra';
import { MyPage } from 'src/app/ui/pages/my/my-page.component';
import { CanDeactivateGuard } from 'src/app/shared/services/general/can-deactivate.service';


@NgModule({
  declarations: [MyPage],
  imports: [
    CommonModule,
    RouterModule.forChild([{ path: '', component: MyPage, canDeactivate: [CanDeactivateGuard] }]),
    UbaselineCoreModule,
  ],
  exports: [
    MyPage
  ],
  entryComponents: [MyPage]
})
export class MyPageModule { }
