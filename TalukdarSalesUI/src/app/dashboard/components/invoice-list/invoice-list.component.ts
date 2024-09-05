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
  selector: 'app-invoice-list',
  templateUrl: './invoice-list.component.html',
  styleUrls: ['./invoice-list.component.scss']
})
export class InvoiceListComponent implements OnInit {

  public collectionHistory:any = [];
  invoiceListDateRange: Date[] = [];

  dateFormat = 'yyyy/MM/dd';
  monthFormat = 'yyyy/MM';
  quarterFormat = 'yyyy/[Q]Q';


  selectedUserGroup: any;
  selectedStatus: any;
  selectedUser: any;
  requisitionNo: string = '';
  selectedInvoiceId: any;
  public productTypes:any = [];
  public invoiceList:any = [];
  public userTypes:any = [];
  public users:any = [];
  public createCollectionForm!: FormGroup;
  createCollectionModal: boolean = false;
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
    const today = new Date(); // Get today's date
    this.invoiceListDateRange = [today, today]; 
    this.getProductTypes();
    this.getInvoiceList();
    this.getUsers();
    this.getUserTypes();
  }

  onDateRangeChangeForInvoiceList(result: Date[]): void {
    this.invoiceListDateRange = result;
    this.getInvoiceList();
  }

  getInvoiceList() {
    this.api.getSalesInvoiceList(this.selectedUser, this.invoiceListDateRange[0], this.invoiceListDateRange[1])
    .subscribe(res => {
      this.invoiceList = res;
      console.log(this.invoiceList);
    });
  }
  

  onFilterChange() {
    this.getInvoiceList();
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

  onUserGroupChange() {
    this.api.getUsersByType(this.selectedUserGroup)
    .subscribe(res=>{
    this.users = res;
    });
  }

  onCreateNew(): void {
    this.router.navigate(['/sales-requisition']);
  }

  openCollectionModal(id: any) {
    this.selectedInvoiceId = id;
    this.createCollectionModal = true;
    this.submitting = false;
    this.createInit();
  }

    private createInit(): void {
    this.createCollectionForm = this.fb.group({
      collectionAmount:[null, Validators.required],
      paymentMethod:['', Validators.required]
    });
  }

  closeModal(){
    this.createCollectionModal = false;
    this.submitting = false;
  }

  getCollectionList(id: any){
    this.router.navigate(['/collection-history', id]);
  }

  onSubmit(){
    if (this.createCollectionForm.valid) {
      this.submitting = true;
      var collectionData = {
        salesInvoiceId: this.selectedInvoiceId,
        collectionAmount: this.createCollectionForm.value.collectionAmount,
        paymentMethod: this.createCollectionForm.value.paymentMethod
      }
      this.api.collectInvoiceAmount(collectionData).subscribe({
        next: (res) => {
          this.createCollectionForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getInvoiceList();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createCollectionForm);
    }
  }

  reset()
  {
    // this.invoiceListDateRange = [];
    // this.selectedUser = null;
    // this.getInvoiceList();
    window.location.reload();
  }
}
