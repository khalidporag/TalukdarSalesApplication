import { Component, OnInit } from '@angular/core';
import { NzModalService } from 'ng-zorro-antd/modal';
import { ApiService } from 'src/app/services/api.service';
import { ModalService } from 'src/app/services/modal.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-landing-page',
  templateUrl: './landing-page.component.html',
  styleUrls: ['./landing-page.component.scss']
})
export class LandingPageComponent implements OnInit {

  noticeDetailsModal = false;
  detailsModalData: any = {};
  public notices: any = [];
  bodyText = 'This text can be updated in modal 1';

  baseUrl = environment.apiBaseUrl;


  constructor(
    private api: ApiService,
    private modalService1: NzModalService,
    protected modalService: ModalService,
  ) { }

  ngOnInit(): void {
    this.getNotices();
  }

  getNotices() {
    this.api.getAllNotices(true)
      .subscribe(res => {
        this.notices = res;
        console.log(res);
      });
  }

  openNoticeDetailsModal(data: any): void {
    this.detailsModalData = data;
    this.noticeDetailsModal = true;
  }

  closeNoticeDetailsModal(): void {
    this.noticeDetailsModal = false;
  }
}
