import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { Product } from 'src/app/models/product.model';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';
@Component({
  selector: 'app-sales-requisition',
  templateUrl: './sales-requisition.component.html',
  styleUrls: ['./sales-requisition.component.scss']
})

export class SalesRequisitionComponent implements OnInit {

  userGroups: string[] = ['Group 1', 'Group 2', 'Group 3'];
  public users: any = [];
  productTypes: string[] = ['Type 1', 'Type 2', 'Type 3'];
  selectedUserGroup: any;
  selectedUser: any;
  selectedProductType: any;

  public userTypes: any = [];

  public salesRequisition: any = [];

  requisitionWithDetail: Product[] = [];
  public products: any = [];

  // public products:any = [];
  public finishedGoodTypes:any = [];
  public createFinishedGoodForm!: FormGroup;
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
    // this.getFinishedGoods();
    this.getUserTypes();
    this.getFinishGoodTypes();
  }

  getUserTypes(){
    this.api.getUserTypes()
    .subscribe(res=>{
    this.userTypes = res;
    console.log(this.userTypes)
    });
  }

  onUserGroupChange() {
    this.api.getUsersByType(this.selectedUserGroup)
    .subscribe(res=>{
    this.users = res;
    });
  }

  onProductTypeChange(){
    this.api.getallFinishedGoodsByType(this.selectedProductType)
    .subscribe(res=>{
    this.products = res;
    console.log(res);
    });
  }

  getUsers(){
    this.api.getUsers()
    .subscribe(res=>{
    this.users = res;
    });
  }

  getFinishedGoods(){
    this.api.getallFinishedGoods()
    .subscribe(res=>{
    this.products = res;
    console.log(res);
    });
  }

  getFinishGoodTypes(){
    this.api.getFinishGoodTypes()
    .subscribe(res=>{
    this.finishedGoodTypes = res;
    console.log(this.finishedGoodTypes)
    });
  }

  private createInit(): void {
    this.createFinishedGoodForm = this.fb.group({
      name:['', Validators.required],
      uOM:['', Validators.required],
      finishedGoodTypeId:[null, Validators.required],
      description:['', Validators.required]
    });
  }

  openFinishedGoodModal() {
    // this.modalService.open('modal-1');
    this.createModal = true;
    this.submitting = false;
    this.getFinishGoodTypes();
    this.createInit();
  }

  closeModal(){
    this.createModal = false;
    this.submitting = false;
  }

  submitRequisition() {
    this.requisitionWithDetail = this.products;
    this.salesRequisition = {
      userId: this.selectedUser,
      goodTypeId: this.selectedProductType,
      requisitionDetails: this.requisitionWithDetail.map(product => ({
        finishedGoodId: product.id,
        price: product.unitPrice,
        quantity: product.quantity
      }))
    };

    this.api.createSalesRequisitionWithDetails(this.salesRequisition).subscribe(response => {
      this.toast.success({detail: "SUCCESS"});
      this.router.navigate(['/requisition-list']);
      console.log('Requisition submitted successfully:', response);
    }, error => {
      this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
      console.error('Error submitting requisition:', error);
    });
  }

  onSubmit(){
    if (this.createFinishedGoodForm.valid) {
      this.submitting = true;
      this.api.createFinishedGood(this.createFinishedGoodForm.value).subscribe({
        next: (res) => {
          this.createFinishedGoodForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getFinishedGoods();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createFinishedGoodForm);
    }
  }
}
