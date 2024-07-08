import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';
@Component({
  selector: 'app-requisition-details',
  templateUrl: './requisition-details.component.html',
  styleUrls: ['./requisition-details.component.scss']
})
export class RequisitionDetailsComponent implements OnInit {
  public collectionHistory:any = [];
  public invoiceList:any = [];
  public requisitionDetailsList:any = [];
  selectedUser: any;
  selectedRequisition: any;
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
    private route: ActivatedRoute,

  ){}

  ngOnInit(): void {
    this.getInvoiceList();
    this.getCollectionList();
    this.selectedRequisition = this.route.snapshot.paramMap.get('id');
    if(this.selectedRequisition!=null){
      this.getRequisitionDetailsList(this.selectedRequisition);
    }
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

  getRequisitionDetailsList(id: any) {
    this.selectedRequisition = id;
    this.api.getSalesRequisitionDetailsList(this.selectedRequisition)
    .subscribe(res => {
      this.requisitionDetailsList = res;
      console.log(this.requisitionDetailsList);
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

