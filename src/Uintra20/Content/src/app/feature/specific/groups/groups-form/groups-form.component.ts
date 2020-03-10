import { Component, Input, HostListener } from '@angular/core';
import { MAX_FILES_FOR_SINGLE } from 'src/app/shared/constants/dropzone/drop-zone.const';
import { IMedia } from '../../activity/activity.interfaces';
import { Router } from '@angular/router';
import { HasDataChangedService } from 'src/app/shared/services/general/has-data-changed.service';
import { TITLE_MAX_LENGTH } from 'src/app/shared/constants/activity/activity-create.const';
import { GroupsService } from '../groups.service';
import { TranslateService } from '@ngx-translate/core';

export interface IEditGroupData {
  id?: string;
  title?: string;
  description?: string;
  media?: {0: string};
  mediaPreview?: {
    medias: Array<IMedia>;
  }
}

@Component({
  selector: 'groups-form',
  templateUrl: './groups-form.component.html',
  styleUrls: ['./groups-form.component.less']
})
export class GroupsFormComponent {
  @Input() data: any;
  @Input() allowedExtensions: string;
  @Input('edit') edit: any;
  @HostListener('window:beforeunload') checkIfDataChanged() {
    return !this.hasDataChangedService.hasDataChanged;
  }
  title: string = "";
  description: string = "";
  medias: string[] = [];
  mediasPreview: IMedia[] = [];
  isShowValidation: boolean = false;
  inProgress: boolean = false;
  hidingInProgress: boolean = false;
  TITLE_MAX_LENGTH: number = TITLE_MAX_LENGTH;
  //File it's array where file[0] is file's object generated by dropzone and file[1] is id
  files: any[] = [];

  constructor(
    private groupsService: GroupsService,
    private router: Router,
    private hasDataChangedService: HasDataChangedService,
    public translate: TranslateService,
  ) { }

  ngOnInit() {
    this.edit = this.edit !== undefined;
    this.setDataToEdit();
  }

  get isSubmitDisabled() {
    return this.inProgress;
  }

  onUploadSuccess(fileArray: Array<any> = []): void {
    this.files.push(fileArray);
    this.hasDataChangedService.onDataChanged();
  }

  onFileRemoved(removedFile: object) {
    this.files = this.files.filter(file => file[0] !== removedFile);
  }

  onImageRemove() {
    this.medias = [];
    this.mediasPreview = [];
    this.hasDataChangedService.onDataChanged();
  }

  onTitleChange(e) {
    if (this.title != e) {
      this.hasDataChangedService.onDataChanged();
    }
    this.title = e;
  }

  onDescriptionChange(e) {
    if (this.description != e) {
      this.hasDataChangedService.onDataChanged();
    }
    this.description = e;
  }

  onSubmit() {
    if (this.validate()) {
      this.inProgress = true;
      const groupModel = {
        title: this.title,
        description: this.description,
        newMedia: this.getMediaIdsForResponse(),
        media: null,
        id: this.data ? this.data.id : null,
      }

      if (!this.edit) {
        this.groupsService.createGroup(groupModel)
        .subscribe(res => {
          this.hasDataChangedService.reset();
          this.router.navigate([res.originalUrl]);
        },
        (err: any) => {
          this.inProgress = false;
        });
      } else {
        if (this.medias && this.medias.length) {
          groupModel.media = this.medias[0];
        }
        this.groupsService.editGroup(groupModel)
        .subscribe(res => {
          this.hasDataChangedService.reset();
          this.router.navigate([res.originalUrl]);
        },
        (err: any) => {
          this.inProgress = false;
        });
      }
    }
  }

  onHide() {
    this.hidingInProgress = true;
    this.groupsService.hideGroup(this.data.id).subscribe(res => {
      this.hasDataChangedService.reset();
      this.router.navigate([res.originalUrl]);
      this.hidingInProgress = false;
    })
  }

  validate(): boolean {
    if (this.title && this.title.length < MAX_FILES_FOR_SINGLE && this.description && this.files.length <= MAX_FILES_FOR_SINGLE) {
      return true;
    }

    this.isShowValidation = true;
    return false;
  }

  getMediaIdsForResponse(): string {
    return this.files.map(file => file[1]).join(',');
  }

  setDataToEdit() {
    if (this.data) {
      this.title = this.data.title;
      this.description = this.data.description;
      this.mediasPreview = this.data.mediaPreview ? Object.values(this.data.mediaPreview.medias) : [];
      this.medias = Object.values(this.data.media);
    }
  }

  getTitleValidationState() {
    return (this.isShowValidation && !this.title) || (this.isShowValidation && this.title.length > TITLE_MAX_LENGTH)
  }

  getTitleLengthValidationMessage() {
    return this.translate.instant('groupEdit.TitleLengthValidation.lbl').replace('{{0}}', TITLE_MAX_LENGTH);
  }
}
