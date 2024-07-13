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

  topFiveSellerDateRange: Date[] = [];
  topFiveSellingProductDateRange: Date[] = [];
  lessFiveSellingProductDateRange: Date[] = [];

  topFiveSellerWithDueAmountDateRange: Date[] = [];

  dailyAccumulatedOrderSummaryDateRange: Date[] = [];



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


  onDateRangeChangeForTopFiveSeller(result: Date[]): void {
    this.topFiveSellerDateRange = result;
    this.getTopFiveSeller();
  }

  getTopFiveSeller() {
    this.api.getTopFiveSeller(this.topFiveSellerDateRange[0], this.topFiveSellerDateRange[1])
      .subscribe(res => {
        this.topFiveSeller = res;
      });
  }


  onDateRangeChangeForTopFiveSellingProduct(result: Date[]): void {
    this.topFiveSellingProductDateRange = result;
    this.getTopFiveSellingProduct();
  }

  getTopFiveSellingProduct() {
    this.api.getTopFiveSellingProduct(this.topFiveSellingProductDateRange[0], this.topFiveSellingProductDateRange[1])
      .subscribe(res => {
        this.topFiveSellingProduct = res;
      });
  }

  onDateRangeChangeForLessFiveSellingProduct(result: Date[]): void {
    this.lessFiveSellingProductDateRange = result;
    this.getLessFiveSellingProduct();
  }

  getLessFiveSellingProduct() {
    this.api.getLessFiveSellingProduct(this.lessFiveSellingProductDateRange[0], this.lessFiveSellingProductDateRange[1])
      .subscribe(res => {
        this.lessFiveSellingProduct = res;
      });
  }

  onDateRangeChangeForTopFiveSellerWithDueAmount(result: Date[]): void {
    this.topFiveSellerWithDueAmountDateRange = result;
    this.getTopFiveSellerWithDueAmount();
  }

  getTopFiveSellerWithDueAmount() {
    this.api.getTopFiveSellerWithDueAmount(this.topFiveSellerWithDueAmountDateRange[0], this.topFiveSellerWithDueAmountDateRange[1])
      .subscribe(res => {
        this.topFiveSellerWithDueAmount = res;
      });
  }

  onDateRangeChangeForDailyAccumulatedOrderSummaryt(result: Date[]): void {
    this.dailyAccumulatedOrderSummaryDateRange = result;
    this.getDailyAccumulatedOrderSummary();
  }

  getDailyAccumulatedOrderSummary() {
    this.api.getDailyAccumulatedOrderSummary(this.dailyAccumulatedOrderSummaryDateRange[0], this.dailyAccumulatedOrderSummaryDateRange[1])
      .subscribe(res => {
        this.dailyAccumulatedOrderSummary = res;
      });
  }
}
