import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';
@Component({
  selector: 'app-requisition-list',
  templateUrl: './requisition-list.component.html',
  styleUrls: ['./requisition-list.component.scss']
})
export class RequisitionListComponent implements OnInit {

  selectedStatus: any;
  selectedUser: any;
  requisitionNo: string = '';

  public productTypes:any = [];
  public requisitionList:any = [];
  public userTypes:any = [];
  public users:any = [];
  public createProductTypeForm!: FormGroup;
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
    private router: Router
  ){}

  ngOnInit(): void {
    this.getProductTypes();
    this.getRequisitionList();
    this.getUsers();
  }

  // getRequisitionList(){
  //   this.api.getSalesRequisitionList()
  //   .subscribe(res=>{
  //   this.requisitionList = res;
  //   console.log(this.requisitionList);
  //   });
  // }

  getRequisitionList() {
    this.api.getSalesRequisitionList(this.selectedStatus, this.selectedUser, this.requisitionNo)
    .subscribe(res => {
      this.requisitionList = res;
    });
  }

  onFilterChange() {
    this.getRequisitionList();
  }

  getProductTypes(){
    this.api.getFinishGoodTypes()
    .subscribe(res=>{
    this.productTypes = res;
    });
  }

  getUsers(){
    this.api.getUsers()
    .subscribe(res=>{
    this.users = res;
    console.log(this.users)
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
    this.createProductTypeForm = this.fb.group({
      name:['', Validators.required]
    });
  }

  openRoleModal() {
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

  onCreateNew(): void {
    this.router.navigate(['/sales-requisition']);
  }

  onSubmit(){
    if (this.createProductTypeForm.valid) {
      this.submitting = true;
      this.api.createFinishedGoodType(this.createProductTypeForm.value).subscribe({
        next: (res) => {
          this.createProductTypeForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getProductTypes();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createProductTypeForm);
    }
  }
}
