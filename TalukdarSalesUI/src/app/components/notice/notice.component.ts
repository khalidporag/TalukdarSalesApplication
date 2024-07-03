import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';

@Component({
  selector: 'app-notice',
  templateUrl: './notice.component.html',
  styleUrls: ['./notice.component.scss']
})
export class NoticeComponent implements OnInit {

  file: File | null = null;
  fileError: string = '';

  public notices:any = [];
  public createNoticeForm!: FormGroup;
  createModal: boolean = false;
  submitting: boolean = false;
  bodyText = 'This text can be updated in modal 1';

  constructor(
    private api : ApiService,
    private modalService1: NzModalService,
    protected modalService: ModalService,
    private auth: AuthService,
    private toast: NgToastService,
    private fb: FormBuilder,
  ){}

  ngOnInit(): void {
    this.getNotices();
  }

  onFileChange(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.file = file;
      this.fileError = '';
    } else {
      this.fileError = 'Please select an image file.';
    }
  }

  getNotices(){
    this.api.getAllNotices()
    .subscribe(res=>{
    this.notices = res;
    });
  }


  private createInit(): void {
    this.createNoticeForm = this.fb.group({
      title:['', Validators.required],
      description:['', Validators.required],
      image: [null, Validators.required]
    });
  }

  openNoticeModal() {
    this.createModal = true;
    this.submitting = false;
    this.createInit();
  }

  closeModal(){
    this.createModal = false;
    this.submitting = false;
  }

  onSubmit(){
    if (this.createNoticeForm.valid) {
      this.submitting = true;

    const formData = new FormData();
    formData.append('title', this.createNoticeForm.value.title);
    formData.append('description', this.createNoticeForm.value.description);

    // Append the image file to the FormData object if it exists
    if (this.file) {
      formData.append('image', this.file, this.file.name);
    }

      this.api.createNotice(formData).subscribe({
        next: (res) => {
          this.createNoticeForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getNotices();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something went wrong!", duration: 5000});
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createNoticeForm);
    }
  }
}
