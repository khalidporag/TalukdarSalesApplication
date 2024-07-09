import { Component, OnInit } from '@angular/core';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { UserStoreService } from 'src/app/services/user-store.service';

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit{
  public users:any = [];
  public role!:string;

  public topFiveSeller:any = [];
  public topFiveSellingProduct:any = [];

  public lessFiveSellingProduct:any = [];

  public topFiveSellerWithDueAmount:any = [];

  public dailyAccumulatedOrderSummary:any = [];




  public fullName : string = "";
  constructor(private api : ApiService, private auth: AuthService, private userStore: UserStoreService) { }

  ngOnInit() {
    this.api.getUsers()
    .subscribe(res=>{
    this.users = res;
    });

    this.getTopFiveSeller();
    this.getTopFiveSellingProduct();
    this.getLessFiveSellingProduct();
    this.getTopFiveSellerWithDueAmount();
    this.getDailyAccumulatedOrderSummary();
  }

  getTopFiveSeller(){
    this.api.getTopFiveSeller()
    .subscribe(res=>{
    this.topFiveSeller = res;
    console.log(this.topFiveSeller);
    });
  }

  getTopFiveSellingProduct(){
    this.api.getTopFiveSellingProduct()
    .subscribe(res=>{
    this.topFiveSellingProduct = res;
    });
  }

  getLessFiveSellingProduct(){
    this.api.getLessFiveSellingProduct()
    .subscribe(res=>{
    this.lessFiveSellingProduct = res;
    });
  }

  getTopFiveSellerWithDueAmount(){
    this.api.getTopFiveSellerWithDueAmount()
    .subscribe(res=>{
    this.topFiveSellerWithDueAmount = res;
    });
  }

  getDailyAccumulatedOrderSummary()
  {
    this.api.getDailyAccumulatedOrderSummary()
    .subscribe(res=>{
    this.dailyAccumulatedOrderSummary = res;
    });
  }
}
