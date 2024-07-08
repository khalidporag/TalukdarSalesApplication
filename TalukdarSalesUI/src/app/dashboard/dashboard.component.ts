import { Component } from '@angular/core';
import { ApiService } from '../services/api.service';
import { UserStoreService } from '../services/user-store.service';
import { AuthService } from '../services/auth.service';
import { CommonService } from '../services/common/common.service';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent {
  // isLoggedIn = this.auth.isLoggedIn();
  isfullArea: boolean = true;
  isVisible = false;

  public fullName: string = "";
  constructor(private api: ApiService, private auth: AuthService,
    private userStore: UserStoreService, private commonService: CommonService) {
    // this.isLoggedIn = this.auth.isLoggedIn();
    // this.userStore.getFullNameFromStore()
    //   .subscribe(val => {
    //     const fullNameFromToken = this.auth.getfullNameFromToken();
    //     this.fullName = val || fullNameFromToken
    //   });
  }
  title = 'TalukdarSalesUI';

  ngOnInit() {
    this.getfullBody();
  }

  getfullBody() {
    this.commonService.dashboardBody$.subscribe((open) => {
      this.isfullArea = !this.isfullArea;
      console.log(this.isfullArea);
    });
  }
}
