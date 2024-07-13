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
  noticeDetailsModal = false;
  detailsModalData: any = {};
  public notices: any = [];
  public createNoticeForm!: FormGroup;
  createModal: boolean = false;
  submitting: boolean = false;
  bodyText = 'This text can be updated in modal 1';

  // Variable - JD
  imageUrl: string | null = null;
  // isEdit: boolean = false;

  baseUrl = "https://localhost:7019/images/notices/"

  cardData: any = [
    {
      logo: "https://t3.ftcdn.net/jpg/01/32/67/54/360_F_132675456_2I1T2Qo0g1fd3o5pUpPv59RUrCH5sbWl.jpg",
      title: "Todays Temparature",
      desc: "The cloud enables users to access the same files and applications from almost any device, because the computing and storage takes place on servers in a data center, instead of locally on the user device. This is why a user can log in to their Instagram account on a new phone after their old phone breaks and still find their old account in place, with all their photos, videos, and conversation history.",
      createdAt: "17 March 2024"
    },
    {
      logo: "https://logomaster.ai/hubfs/gallery002.png",
      title: "Todays Temparature",
      desc: "The cloud enables users to access the same files and applications from almost any device, because the computing and storage takes place on servers in a data center, instead of locally on the user device. This is why a user can log in to their Instagram account on a new phone after their old phone breaks and still find their old account in place, with all their photos, videos, and conversation history.",
      createdAt: "17 March 2024"
    },
    {
      logo: "https://www.edigitalagency.com.au/wp-content/uploads/ikea-logo-png.png",
      title: "Todays Temparature",
      desc: "The cloud enables users to access the same files and applications from almost any device, because the computing and storage takes place on servers in a data center, instead of locally on the user device. This is why a user can log in to their Instagram account on a new phone after their old phone breaks and still find their old account in place, with all their photos, videos, and conversation history.",
      createdAt: "17 March 2024"
    },
    {
      logo: "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTL3y9HMdddArZOshBzTKmM4pln2fHcn6_2JQ&s",
      title: "Todays Temparature",
      desc: "The cloud enables users to access the same files and applications from almost any device, because the computing and storage takes place on servers in a data center, instead of locally on the user device. This is why a user can log in to their Instagram account on a new phone after their old phone breaks and still find their old account in place, with all their photos, videos, and conversation history.",
      createdAt: "17 March 2024"
    }
  ]

  constructor(
    private api: ApiService,
    private modalService1: NzModalService,
    protected modalService: ModalService,
    private auth: AuthService,
    private toast: NgToastService,
    private fb: FormBuilder,
  ) { }

  ngOnInit(): void {
    this.getNotices();
  }

  // Old Code Numan
  // onFileChange(event: any): void {
  //   const file = event.target.files[0];
  //   if (file) {
  //     this.file = file;
  //     this.fileError = '';
  //   } else {
  //     this.fileError = 'Please select an image file.';
  //   }
  // }

  // New Code Joydip
  onFileChange(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.file = file;
      const reader = new FileReader();
      reader.onload = (e: any) => {
        this.imageUrl = e.target.result;
      };
      reader.readAsDataURL(file);
      this.fileError = '';
    } else {
      this.fileError = 'Please select an image file.';
    }
  }

  changeImage(): void {
    this.imageUrl = null;
  }
  // New Code Joydip

  getNotices() {
    this.api.getAllNotices()
      .subscribe(res => {
        this.notices = res;
      });
  }


  private createInit(): void {
    this.createNoticeForm = this.fb.group({
      title: ['', Validators.required],
      description: ['', Validators.required],
      image: [null, Validators.nullValidator]
    });
  }

  openNoticeModal() {
    this.createModal = true;
    this.submitting = false;
    this.createInit();
  }

  closeModal() {
    this.createModal = false;
    this.submitting = false;
  }

  onSubmit() {
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
          this.toast.success({ detail: "SUCCESS", summary: res.message, duration: 5000 });
          this.closeModal();
          this.getNotices();
        },
        error: (err) => {
          this.toast.error({ detail: "ERROR", summary: "Something went wrong!", duration: 5000 });
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createNoticeForm);
    }
  }

  // imageUrl: any;
  openNoticeDetailsModal(data: any): void {
    this.detailsModalData = data;
    this.noticeDetailsModal = true;
    // this.imageUrl = this.baseUrl + data.logoName;
    // this.detailsModalData.logoName = this.imageUrl;
    // console.log(this.imageUrl);
  }

  closeNoticeDetailsModal(): void {
    this.noticeDetailsModal = false;
  }
}
