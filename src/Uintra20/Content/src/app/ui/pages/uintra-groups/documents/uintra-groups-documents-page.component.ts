import { Component, ViewEncapsulation, ViewChild } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { UintraGroupsService } from "./uintra-groups-documents-page.service";
import { AddButtonService } from 'src/app/ui/main-layout/left-navigation/components/my-links/add-button.service';
import { DropzoneWrapperComponent } from 'src/app/feature/reusable/ui-elements/dropzone-wrapper/dropzone-wrapper.component';
import ParseHelper from 'src/app/shared/utils/parse.helper';

@Component({
  selector: "uintra-groups-documents-page",
  templateUrl: "./uintra-groups-documents-page.html",
  styleUrls: ["./uintra-groups-documents-page.less"],
  encapsulation: ViewEncapsulation.None
})
export class UintraGroupsDocumentsPage {
  @ViewChild(DropzoneWrapperComponent, { static: false })
  dropdownRef: DropzoneWrapperComponent;
  data: any;
  parsedData: any;
  // File it's array where file[0] is file's object generated by dropzone and file[1] is id
  file: any = null;
  hasUploadPossible: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private uintraGroupsService: UintraGroupsService,
    private addButtonService: AddButtonService
  ) {
    this.route.data.subscribe(data => {
      this.data = data;
      this.parsedData = ParseHelper.parseUbaselineData(this.data);
      this.addButtonService.setPageId(data.id);
    });
  }

  onUploadSuccess(file) {
    this.hasUploadPossible = true;
    this.file = file;
  }

  onUploadFile() {
    const fileId = this.file[1];

    if (fileId) {
      this.uintraGroupsService
        .uploadFile(fileId, this.data.groupId.get())
        .subscribe(r => {
          this.uintraGroupsService.refreshDocuments();
          this.file = null;
          this.dropdownRef.handleReset();
        });
    }
  }
}
