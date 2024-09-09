import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { SalesInvoiceDto } from 'src/app/models/invoice-list.model';
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
  //public invoiceList: any = [];
  public invoiceList: SalesInvoiceDto[] = [];
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
    const today = new Date(); // Get today's date
    this.collectionHistoryDateRange = [today, today]; 
    this.getUserTypes();
    this.getInvoiceList();
    this.selectedInvoice = this.route.snapshot.paramMap.get('id');
    this.getCollectionList();
  }

  getInvoiceList() {
    this.api.getSalesInvoiceListForCollection()
      .subscribe(res => {
        // this.invoiceList = res;
        this.invoiceList = res.salesInvoiceInfo || [];
        console.log(this.invoiceList)
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
                console.log('Collection History Response:', res);
                this.collectionHistory = res;
            });
    } else {
        this.api.getCollectionHistory(this.collectionHistoryDateRange[0], this.collectionHistoryDateRange[1], this.selectedUser, this.selectedInvoice)
            .subscribe(res => {
                console.log('Collection History Response:', res);
                this.collectionHistory = res;
            });
    }
}

getUserTypes() {
    this.api.getUserTypes()
      .subscribe(res => {
        this.userTypes = res;
      });
  }

  reset() {
    window.location.reload();
  }
}
