import { Component } from '@angular/core';
import { AuthService } from './services/auth.service';
import { ApiService } from './services/api.service';
import { UserStoreService } from './services/user-store.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  public fullName : string = "";
  constructor(private api : ApiService, private auth: AuthService,
     private userStore: UserStoreService) {
      this.userStore.getFullNameFromStore()
    .subscribe(val=>{
      const fullNameFromToken = this.auth.getfullNameFromToken();
      this.fullName = val || fullNameFromToken
    });
      }
  title = 'TalukdarSalesUI';

  logout(){
    this.auth.signOut();
  }
}
