import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit {
  baseUrl = environment.apiBaseUrl;

  nameSearch: string = '';
  selectedUser: any;


  file: File | null = null;
  fileError: string = '';
  selectedUserType: any;
  selectedRole: any;
  imageUrl: any;
  public roles: any = [];
  public users: any = [];
  public userTypes: any = [];
  public createUserForm!: FormGroup;
  public updateUserForm!: FormGroup;
  createModal: boolean = false;
  updateModal: boolean = false;
  submitting: boolean = false;
  bodyText = 'This text can be updated in modal 1';

  userData: any = [
    {
      thumb: "",
      fname: "Nancy",
      lname: "Martino",
      phone: "+8801712345678",
      username: "nanchy9978",
      maxCredit: "10",
      dueAmount: "1500",
      userType: "Admin"
    },
    {
      thumb: "https://westernfinance.org/wp-content/uploads/speaker-3-v2.jpg",
      fname: "Nancy",
      lname: "Martino",
      phone: "+8801712345678",
      username: "nanchy9978",
      maxCredit: "10",
      dueAmount: "1500",
      userType: "Admin"
    },
    {
      thumb: "",
      fname: "Nancy",
      lname: "Martino",
      phone: "+8801712345678",
      username: "nanchy9978",
      maxCredit: "10",
      dueAmount: "1500",
      userType: "Admin"
    },
    {
      thumb: "https://images.pexels.com/photos/1486974/pexels-photo-1486974.jpeg",
      fname: "Nancy",
      lname: "Martino",
      phone: "+8801712345678",
      username: "nanchy9978",
      maxCredit: "10",
      dueAmount: "1500",
      userType: "Admin"
    },
    {
      thumb: "https://images.pexels.com/photos/1486974/pexels-photo-1486974.jpeg",
      fname: "Nancy",
      lname: "Martino",
      phone: "+8801712345678",
      username: "nanchy9978",
      maxCredit: "10",
      dueAmount: "1500",
      userType: "Admin"
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
    this.getUsers();
    this.getUserTypes();
    this.getRoles();
  }

  // onFileChange(event: any): void {
  //   const file = event.target.files[0];
  //   if (file) {
  //     this.file = file;
  //     this.fileError = '';
  //   } else {
  //     this.fileError = 'Please select an image file.';
  //   }
  // }

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

  getRoles() {
    this.api.getRoles()
      .subscribe(res => {
        this.roles = res;
      });
  }

  getUsers() {
    this.api.getUsers(this.selectedUserType, this.nameSearch)
      .subscribe(res => {
        this.users = res;
      });
  }

  getUserTypes() {
    this.api.getUserTypes()
      .subscribe(res => {
        this.userTypes = res;
        console.log(this.userTypes)
      });
  }

  private createUserInit(): void {
    this.createUserForm = this.fb.group({
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      userTypeId: [null, Validators.required],
      roleId: [null, Validators.required],
      // userName: ['', Validators.required],
      phoneNumber: ['', Validators.required],
      maxCreditLimit: [null, Validators.required],
      image: [null, Validators.nullValidator]
    });
  }

  private editUserInit(): void {
    this.updateUserForm = this.fb.group({
      firstName: [this.selectedUser.firstName || '', Validators.required],
      lastName: [this.selectedUser.lastName || '', Validators.required],
      maxCreditLimit: [this.selectedUser.maxCreditLimit || null, Validators.required],
      // image: [null, Validators.nullValidator]
    });
  }

  openUserModal() {
    // this.modalService.open('modal-1');
    this.createModal = true;
    this.submitting = false;
    this.getUserTypes();
    this.createUserInit();
  }

  openUserEditModal(item: any) {
    // this.modalService.open('modal-1');
    this.updateModal = true;
    this.selectedUser = item;
    this.submitting = false;
    this.editUserInit();
  }

  closeModal() {
    this.createModal = false;
    this.updateModal = false;
    this.submitting = false;
  }

  onSubmit() {
    if (this.createUserForm.valid) {
      this.submitting = true;

      const formData = new FormData();
      formData.append('firstName', this.createUserForm.value.firstName);
      formData.append('lastName', this.createUserForm.value.lastName);
      formData.append('roleId', this.createUserForm.value.roleId);
      formData.append('userTypeId', this.createUserForm.value.userTypeId);
      // formData.append('userName', this.createUserForm.value.userName);
      formData.append('phoneNumber', this.createUserForm.value.phoneNumber);
      formData.append('maxCreditLimit', this.createUserForm.value.maxCreditLimit);

      // Append the image file to the FormData object if it exists
      if (this.file) {
        formData.append('image', this.file, this.file.name);
      }

      this.auth.signUp(formData).subscribe({
        next: (res) => {
          this.createUserForm.reset();
          this.toast.success({ detail: "SUCCESS", summary: res.message, duration: 5000 });
          this.closeModal();
          this.getUsers();
        },
        error: (err) => {
          this.toast.error({ detail: "ERROR", summary: "Something when wrong!", duration: 5000 });
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createUserForm);
    }
  }

  updateUser() {
    if (this.updateUserForm.valid) {
      this.submitting = true;

      const formData = new FormData();
      formData.append('id', this.selectedUser.id);
      formData.append('firstName', this.updateUserForm.value.firstName);
      formData.append('lastName', this.updateUserForm.value.lastName);
      formData.append('maxCreditLimit', this.updateUserForm.value.maxCreditLimit);

      // Append the image file to the FormData object if it exists
      // if (this.file) {
      //   formData.append('image', this.file, this.file.name);
      // }

      this.api.updateUser(formData).subscribe({
        next: (res) => {
          this.updateUserForm.reset();
          this.toast.success({ detail: "SUCCESS", summary: res.message, duration: 5000 });
          this.closeModal();
          this.getUsers();
        },
        error: (err) => {
          this.toast.error({ detail: "ERROR", summary: "Something when wrong!", duration: 5000 });
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createUserForm);
    }
  }
}
