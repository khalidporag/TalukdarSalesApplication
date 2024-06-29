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

  getNotices(){
    this.api.getAllNotices()
    .subscribe(res=>{
    this.notices = res;
    });
  }


  private createInit(): void {
    this.createNoticeForm = this.fb.group({
      title:['', Validators.required],
      description:['', Validators.required]
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
      this.api.createNotice(this.createNoticeForm.value).subscribe({
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
