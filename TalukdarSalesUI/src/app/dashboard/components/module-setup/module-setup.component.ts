import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';
@Component({
  selector: 'app-module-setup',
  templateUrl: './module-setup.component.html',
  styleUrls: ['./module-setup.component.scss']
})
export class ModuleSetupComponent implements OnInit {
  public applicationModules:any = [];
  public userTypes:any = [];
  public addApplicationModuleForm!: FormGroup;
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
    this.getApplicationModuleList();
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
    this.addApplicationModuleForm = this.fb.group({
      name:['', Validators.required],
      url:['', Validators.required]
    });
  }

  openAppModuleModal() {
    // this.modalService.open('modal-1');
    this.createModal = true;
    this.submitting = false;
    this.getUserTypes();
    this.createInit();
  }

  closeModal(){
    this.createModal = false;
    this.submitting = false;
  }

  onSubmit(){
    if (this.addApplicationModuleForm.valid) {
      this.submitting = true;
      this.api.createModule(this.addApplicationModuleForm.value).subscribe({
        next: (res) => {
          this.addApplicationModuleForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getApplicationModuleList();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.addApplicationModuleForm);
    }
  }
}
