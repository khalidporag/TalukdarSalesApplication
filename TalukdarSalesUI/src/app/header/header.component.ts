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
        } else if (event['url'] == '/user-management') {
          this.pageTitle = 'User Management';
        }
        else if (event['url'] == '/role') {
          this.pageTitle = 'Role Management';
        }
        else if (event['url'] == '/report') {
          this.pageTitle = 'Report';
        }
        else if (event['url'] == '/notice') {
          this.pageTitle = 'Notice';
        }
        else if (event['url'] == '/sales-invoice') {
          this.pageTitle = 'Invice Form';
        }
        else if (event['url'] == '/invoice-list') {
          this.pageTitle = 'Invoice List';
        }
        else if (event['url'] == '/product-type') {
          this.pageTitle = 'Product Type Setup';
        }
        else if (event['url'] == '/product') {
          this.pageTitle = 'Product Setup';
        }
        else if (event['url'] == '/time-setting') {
          this.pageTitle = 'Time Setting';
        }
        else if (event['url'] == '/productionwise-requisition') {
          this.pageTitle = 'Production Planning';
        }
        else if (event['url'] == '/collection-history') {
          this.pageTitle = 'Collection History';
        }
        else {
          this.pageTitle = 'Talukdar Sales';
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
