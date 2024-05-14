import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';

@Component({
  selector: 'app-user-management',
  templateUrl: './user-management.component.html',
  styleUrls: ['./user-management.component.scss']
})
export class UserManagementComponent implements OnInit {
  public users:any = [];
  public userTypes:any = [];
  public createUserForm!: FormGroup;
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
    this.getUsers();
  }

  getUsers(){
    this.api.getUsers()
    .subscribe(res=>{
    this.users = res;
    });
  }

  getUserTypes(){
    this.api.getUserTypes()
    .subscribe(res=>{
    this.userTypes = res;
    console.log(this.userTypes)
    });
  }

  private createUserInit(): void {
    this.createUserForm = this.fb.group({
      firstName:['', Validators.required],
      lastName:['', Validators.required],
      userTypeId:[null, Validators.required],
      userName:['', Validators.required],
      phoneNumber:['', Validators.required],
      maxCreditLimit:[null, Validators.required]
    });
  }

  openUserModal() {
    // this.modalService.open('modal-1');
    this.createModal = true;
    this.submitting = false;
    this.getUserTypes();
    this.createUserInit();
  }

  closeModal(){
    this.createModal = false;
    this.submitting = false;
  }

  onSubmit(){
    if (this.createUserForm.valid) {
      this.submitting = true;
      this.auth.signUp(this.createUserForm.value).subscribe({
        next: (res) => {
          this.createUserForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getUsers();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createUserForm);
    }
  }
}
