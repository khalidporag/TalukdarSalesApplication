import { Component } from '@angular/core';
import { AuthService } from './services/auth.service';
import { ApiService } from './services/api.service';
import { UserStoreService } from './services/user-store.service';
import { CommonService } from './services/common/common.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  isLoggedIn = this.auth.isLoggedIn();
  isfullArea: boolean = true;
  isVisible = false;

  public fullName: string = "";
  constructor(private api: ApiService, private auth: AuthService,
    private userStore: UserStoreService, private commonService: CommonService) {
    this.isLoggedIn = this.auth.isLoggedIn();
    this.userStore.getFullNameFromStore()
      .subscribe(val => {
        const fullNameFromToken = this.auth.getfullNameFromToken();
        this.fullName = val || fullNameFromToken
      });
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

  logout() {
    this.auth.signOut();
    window.location.reload();
  }
}
