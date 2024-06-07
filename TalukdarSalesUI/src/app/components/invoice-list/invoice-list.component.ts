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

  selectedStatus: any;
  selectedUser: any;
  requisitionNo: string = '';

  public productTypes:any = [];
  public invoiceList:any = [];
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
    this.getInvoiceList();
    this.getUsers();
  }

  getInvoiceList() {
    this.api.getSalesInvoiceList(this.selectedUser)
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

  onCreateNew(): void {
    this.router.navigate(['/sales-requisition']);
  }
}
