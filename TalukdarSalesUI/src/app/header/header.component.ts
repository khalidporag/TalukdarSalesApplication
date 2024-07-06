import { Component } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { CommonService } from '../services/common/common.service';
import { ConfigService } from '../services/common/config.service';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent {
  isHeaderFullArea: boolean = false;
  pageTitle: string = '';
  parentPage: string = '';
  userData: any = [];
  userInfoModal: boolean = false;
  idom: string = this.config.imageURL;

  constructor(private commonService: CommonService, private router: Router, public config: ConfigService) {
    router.events.forEach((event) => {
      if (event instanceof NavigationEnd) {
        this.pageTitle = event.url;
        if (event['url'] == '/') {
          this.pageTitle = 'Talukdar Sales';
          this.parentPage = 'Dashboard';
        } else if (event['url'] == '/user-management') {
          this.pageTitle = 'User Management';
          this.parentPage = 'Table';
        }
        else if (event['url'] == '/deals-list') {
          this.pageTitle = 'Deal List';
          this.parentPage = 'Deal Management';
        }
        else if (event['url'].startsWith('/vendor-list/')) {
          this.pageTitle = 'Vendor Details';
          this.parentPage = 'Vendor List';
        }
        else if (event['url'].startsWith('/vendors/')) {
          this.pageTitle = 'Review';
          this.parentPage = 'Review Management';
        }
        else if (event['url'] == '/vendor-list') {
          this.pageTitle = 'Vendor List';
          this.parentPage = 'Vendor Management';
        }
        else if (event['url'].startsWith('/deals-list/')) {
          this.pageTitle = 'Deal Details';
          this.parentPage = 'Deal List';
        }
        else if (event['url'] == '/location') {
          this.pageTitle = 'Location';
          this.parentPage = 'Location Management';
        }
        else if (event['url'].startsWith('/location/')) {
          this.pageTitle = 'Area List';
          this.parentPage = 'Area';
        }
        else if (event['url'] == '/category') {
          this.pageTitle = 'Category';
          this.parentPage = 'Category Management';
        }
        else if (event['url'] == '/feeds') {
          this.pageTitle = 'Feed List';
          this.parentPage = 'Feeds Management';
        }
        else if (event['url'].startsWith('/feeds/')) {
          this.pageTitle = 'Feeds Details';
          this.parentPage = 'Feeds List';
        }
        else if (event['url'] == '/user-list') {
          this.pageTitle = 'User List';
          this.parentPage = 'User Management';
        }
        else if (event['url'].startsWith('/user-list/')) {
          this.pageTitle = 'User Details';
          this.parentPage = 'User List';
        }
        else if (event['url'] == '/all-sms') {
          this.pageTitle = 'Sms List';
          this.parentPage = 'Sms Management';
        }
        else if (event['url'] == '/review') {
          this.pageTitle = 'Review List';
          this.parentPage = 'Review Management';
        }
        else if (event['url'].startsWith('/review/')) {
          this.pageTitle = 'Review List';
          this.parentPage = 'Review Management';
        }
        else if (event['url'] == '/visited-history') {
          this.pageTitle = 'Visited Data';
          this.parentPage = 'Visited History';
        }
        else if (event['url'] == '/voucher-list') {
          this.pageTitle = 'Voucher';
          this.parentPage = 'Voucher List';
        }
        else {
          // console.log(event['url']);
          this.pageTitle = 'Talukdar Sales';
          this.parentPage = 'Talukdar Sales';
        }
      }
    });
  }

  ngOnInit() {
  }

  getfullHeader() {
    this.commonService.dashboardBody$.subscribe((open) => {
      this.isHeaderFullArea = !this.isHeaderFullArea;
      console.log(this.isHeaderFullArea);
    });
  }

  openSidebar() {
    this.commonService.toggleSidebar();
    this.commonService.toggleBody();
    this.commonService.headerState$.subscribe((open) => {
      this.isHeaderFullArea = !this.isHeaderFullArea;
      console.log(this.isHeaderFullArea);
    });
  }

  openSidebarMobile() {
    this.commonService.toggleSidebarMobile();
  }

  openUserModal() {
    this.userInfoModal = true;
  }

  closeModal() {
    this.userInfoModal = false;
  }
}
