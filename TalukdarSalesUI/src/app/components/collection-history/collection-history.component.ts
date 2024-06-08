import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';
@Component({
  selector: 'app-collection-history',
  templateUrl: './collection-history.component.html',
  styleUrls: ['./collection-history.component.scss']
})
export class CollectionHistoryComponent implements OnInit {
  public collectionHistory:any = [];
  public invoiceList:any = [];
  selectedUser: any;
  selectedInvoice: any;
  public userTypes:any = [];
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
  ){}

  ngOnInit(): void {
    this.getInvoiceList();
    this.getCollectionList();
  }

  getInvoiceList() {
    this.api.getSalesInvoiceList()
    .subscribe(res => {
      this.invoiceList = res;
      console.log(this.invoiceList);
    });
  }

  getCollectionList(){
    this.api.getCollectionHistory(this.selectedUser, this.selectedInvoice)
    .subscribe(res=>{
    this.collectionHistory = res;
    });
  }

  getUserTypes(){
    this.api.getUserTypes()
    .subscribe(res=>{
    this.userTypes = res;
    console.log(this.userTypes)
    });
  }
}
