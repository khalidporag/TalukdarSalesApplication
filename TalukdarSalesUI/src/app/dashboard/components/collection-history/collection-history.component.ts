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
  selector: 'app-collection-history',
  templateUrl: './collection-history.component.html',
  styleUrls: ['./collection-history.component.scss']
})
export class CollectionHistoryComponent implements OnInit {
  public collectionHistory: any = [];
  public invoiceList: any = [];
  collectionHistoryDateRange: Date[] = [];

  selectedUser: any;
  selectedInvoice: any;
  public userTypes: any = [];
  public createProductTypeForm!: FormGroup;
  createModal: boolean = false;
  submitting: boolean = false;
  bodyText = 'This text can be updated in modal 1';

  dateFormat = 'yyyy/MM/dd';
  monthFormat = 'yyyy/MM';
  quarterFormat = 'yyyy/[Q]Q';

  constructor(
    private api: ApiService,
    private modalService1: NzModalService,
    protected modalService: ModalService,
    private auth: AuthService,
    private toast: NgToastService,
    private fb: FormBuilder,
    private route: ActivatedRoute
  ) { }

  ngOnInit(): void {
    this.getUserTypes();
    this.getInvoiceList();
    this.selectedInvoice = this.route.snapshot.paramMap.get('id');
    this.getCollectionList();
  }

  getInvoiceList() {
    this.api.getSalesInvoiceList()
      .subscribe(res => {
        this.invoiceList = res;
        console.log(this.invoiceList);
      });
  }

  onDateRangeChangeForCollection(result: Date[]): void {
    this.collectionHistoryDateRange = result;
    this.getCollectionList();
  }

  getCollectionList() {
    if (this.selectedInvoice == null) {
      this.api.getCollectionHistory(this.collectionHistoryDateRange[0], this.collectionHistoryDateRange[1], this.selectedUser)
        .subscribe(res => {
          this.collectionHistory = res;
          console.log(res);
        });
    }
    this.api.getCollectionHistory(this.collectionHistoryDateRange[0], this.collectionHistoryDateRange[1], this.selectedUser, this.selectedInvoice)
      .subscribe(res => {
        this.collectionHistory = res;

        // const totalCollectionAmount = this.collectionHistory.reduce((sum, item) => {
        //     return sum + item.collectionAmount;
        // }, 0);
        // console.log('Total Collection Amount:', totalCollectionAmount);

      });
  }

  getUserTypes() {
    this.api.getUserTypes()
      .subscribe(res => {
        this.userTypes = res;
        console.log(this.userTypes)
      });
  }

  reset() {
    window.location.reload();
  }
}
