import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { RequisitionInfo } from 'src/app/models/requisition-info.model';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';
@Component({
  selector: 'app-requisition-list',
  templateUrl: './requisition-list.component.html',
  styleUrls: ['./requisition-list.component.scss']
})
export class RequisitionListComponent implements OnInit {

  selectedStatus: boolean = true;
  selectedUser: any;
  requisitionNo: string = '';
  selectedRequisition: any;

  public productTypes:any = [];
  // public requisitionList= [];
  requisitionList: RequisitionInfo[] = [];
  public requisitionDetailsList:any = [];
  public userTypes:any = [];
  public users:any = [];
  public createProductTypeForm!: FormGroup;
  createModal: boolean = false;
  submitting: boolean = false;
  bodyText = 'This text can be updated in modal 1';
  selectAll: boolean = false;


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

  toggleSelectAll(event: any) {
    this.selectAll = event.target.checked;
    this.requisitionList.forEach(requisition => requisition.selected = this.selectAll);
  }

  onCheckboxChange() {
    this.selectAll = this.requisitionList.every(requisition => requisition.selected);
  }

  sendSelectedRequisitions() {
    // const selectedIds = this.requisitionList.filter(requisition => requisition.selected).map(requisition => requisition.id);
    // this.http.post('YOUR_BACKEND_API_ENDPOINT', { ids: selectedIds }).subscribe(response => {
    //   // Handle response
    // });
  }

  getRequisitionList() {
    this.api.getSalesRequisitionList(this.selectedStatus, this.selectedUser, this.requisitionNo)
    .subscribe(res => {
      this.requisitionList = res;
      console.log(this.requisitionList);
    });
  }

  getRequisitionDetailsList(id: any) {
    this.selectedRequisition = id;
    this.api.getSalesRequisitionDetailsList(this.selectedRequisition)
    .subscribe(res => {
      this.requisitionDetailsList = res;
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

  onCreateNew(): void {
    this.router.navigate(['/sales-requisition']);
  }
}
