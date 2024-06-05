import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';

@Component({
  selector: 'app-role',
  templateUrl: './role.component.html',
  styleUrls: ['./role.component.scss']
})
export class RoleComponent implements OnInit {
  public roles:any = [];
  public applicationModules:any = [];
  public userTypes:any = [];
  public createRoleForm!: FormGroup;
  public addModuleToRoleForm!: FormGroup;

  createModal: boolean = false;
  addModuleToRoleModal: boolean = false;
  addModule: boolean = false;
  submitting: boolean = false;
  bodyText = 'This text can be updated in modal 1';

  newItem: string = '';
  items: { label: string, checked: boolean }[] = [];

  addItem() {
    if (this.newItem.trim() !== '') {
      this.items.push({ label: this.newItem, checked: false });
      this.newItem = '';
    }
  }

  constructor(
    private api : ApiService,
    private modalService1: NzModalService,
    protected modalService: ModalService,
    private auth: AuthService,
    private toast: NgToastService,
    private fb: FormBuilder,
  ){
    this.addModuleToRoleForm = this.fb.group({});

  }

  ngOnInit(): void {
    this.getRoles();
    this.getApplicationModuleList();
  }

  getRoles(){
    this.api.getRoles()
    .subscribe(res=>{
    this.roles = res;
    });
  }

  getApplicationModuleList(){
    this.api.getAllModules()
    .subscribe(res=>{
    this.applicationModules = res;
    });
  }

  getUserTypes(){
    this.api.getUserTypes()
    .subscribe(res=>{
    this.userTypes = res;
    console.log(this.userTypes)
    });
  }

  private createInit(): void {
    this.createRoleForm = this.fb.group({
      name:['', Validators.required]
    });
  }

  private addModuleToRole(): void{
    this.addModuleToRoleForm = this.fb.group({
      name:['', Validators.required],
      moduleId:['', Validators.nullValidator]
    });
  }

  openRoleModal() {
    // this.modalService.open('modal-1');
    this.createModal = true;
    this.submitting = false;
    this.getUserTypes();
    this.createInit();
  }

  openAddModuleModal() {
    // this.modalService.open('modal-1');
    this.addModuleToRoleModal = true;
    this.submitting = false;
    this.getApplicationModuleList();
    console.log(this.applicationModules);
    this.addModuleToRole();
  }

  closeModal(){
    this.createModal = false;
    this.addModuleToRoleModal = false;
    this.submitting = false;
  }

  onSubmit(){
    if (this.createRoleForm.valid) {
      this.submitting = true;
      this.api.createRole(this.createRoleForm.value).subscribe({
        next: (res) => {
          this.createRoleForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getRoles();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createRoleForm);
    }
  }

  assignModuleToRole()
  {
    this.submitting = true;
  }
  onModuleSubmitToRole(){
    if (this.createRoleForm.valid) {
      this.submitting = true;
      this.api.createRole(this.createRoleForm.value).subscribe({
        next: (res) => {
          this.createRoleForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getRoles();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createRoleForm);
    }
  }
}
