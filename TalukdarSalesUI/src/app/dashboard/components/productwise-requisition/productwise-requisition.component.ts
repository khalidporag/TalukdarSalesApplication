import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';
@Component({
  selector: 'app-productwise-requisition',
  templateUrl: './productwise-requisition.component.html',
  styleUrls: ['./productwise-requisition.component.scss']
})
export class ProductwiseRequisitionComponent implements OnInit {
  public productList:any = [];
  constructor(
    private api : ApiService
  ){}

  ngOnInit(): void {
    this.getProductionPlanning();
  }

  getProductionPlanning() {
    this.api.getProductWiseDailyRequisition()
    .subscribe(res => {
      this.productList = res;
    });
  }
}
