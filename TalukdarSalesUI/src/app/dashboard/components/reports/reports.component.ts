import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { UserStoreService } from 'src/app/services/user-store.service';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {
  dateFormat = 'yyyy/MM/dd';
  monthFormat = 'yyyy/MM';
  quarterFormat = 'yyyy/[Q]Q';

  fromDateTopSellers: any;
  toDateTopSellers: any;
  fromDateTopProducts: any;
  toDateTopProducts: any;
  fromDateLessProducts: any;
  toDateLessProducts: any;
  fromDateSellersDue: any;
  toDateSellersDue: any;
  fromDateAccumulatedOrders: any;
  toDateAccumulatedOrders: any;

  public users: any = [];
  public role!: string;

  public topFiveSeller: any = [];
  public topFiveSellingProduct: any = [];

  public lessFiveSellingProduct: any = [];

  public topFiveSellerWithDueAmount: any = [];

  public dailyAccumulatedOrderSummary: any = [];




  public fullName: string = "";
  constructor(private api: ApiService, private auth: AuthService, private userStore: UserStoreService) { }

  ngOnInit() {
    this.api.getUsers()
      .subscribe(res => {
        this.users = res;
      });

    this.getTopFiveSeller();
    this.getTopFiveSellingProduct();
    this.getLessFiveSellingProduct();
    this.getTopFiveSellerWithDueAmount();
    this.getDailyAccumulatedOrderSummary();
  }

  onDateChange(listType: string) {
    switch (listType) {
      case 'topSellers':
        this.getTopFiveSeller();
        break;
      case 'topProducts':
        this.getTopFiveSellingProduct();
        break;
      case 'lessProducts':
        this.getLessFiveSellingProduct();
        break;
      case 'sellersDue':
        this.getTopFiveSellerWithDueAmount();
        break;
      case 'accumulatedOrders':
        this.getDailyAccumulatedOrderSummary();
        break;
      default:
        break;
    }
  }

  getTopFiveSeller() {
    this.api.getTopFiveSeller(this.fromDateTopSellers, this.toDateTopSellers)
      .subscribe(res => {
        this.topFiveSeller = res;
        console.log(this.topFiveSeller);
      });
  }

  getTopFiveSellingProduct() {
    this.api.getTopFiveSellingProduct()
      .subscribe(res => {
        this.topFiveSellingProduct = res;
      });
  }

  getLessFiveSellingProduct() {
    this.api.getLessFiveSellingProduct()
      .subscribe(res => {
        this.lessFiveSellingProduct = res;
      });
  }

  getTopFiveSellerWithDueAmount() {
    this.api.getTopFiveSellerWithDueAmount()
      .subscribe(res => {
        this.topFiveSellerWithDueAmount = res;
      });
  }

  getDailyAccumulatedOrderSummary() {
    this.api.getDailyAccumulatedOrderSummary()
      .subscribe(res => {
        this.dailyAccumulatedOrderSummary = res;
      });
  }
}
