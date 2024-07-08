import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { DashboardRoutingModule } from './dashboard-routing.module';
import { DashboardComponent } from './dashboard.component';
import { SidebarAreaComponent } from '../sidebar-area/sidebar-area.component';
import { HeaderComponent } from '../header/header.component';
import { UserManagementComponent } from './components/user-management/user-management.component';
import { ModalComponent } from './components/modal/modal.component';
import { ProductComponent } from './components/product/product.component';
import { RoleComponent } from './components/role/role.component';
import { ProductTypeComponent } from './components/product-type/product-type.component';
import { ModuleSetupComponent } from './components/module-setup/module-setup.component';
import { SalesRequisitionComponent } from './components/sales-requisition/sales-requisition.component';
import { RequisitionListComponent } from './components/requisition-list/requisition-list.component';
import { SalesInvoiceComponent } from './components/sales-invoice/sales-invoice.component';
import { InvoiceListComponent } from './components/invoice-list/invoice-list.component';
import { CollectionHistoryComponent } from './components/collection-history/collection-history.component';
import { RequisitionDetailsComponent } from './components/requisition-details/requisition-details.component';
import { ProductwiseRequisitionComponent } from './components/productwise-requisition/productwise-requisition.component';
import { TimeSettingComponent } from './components/time-setting/time-setting.component';
import { NoticeComponent } from './components/notice/notice.component';
import { SettingsComponent } from './components/settings/settings.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../shared/shared.module';
import { NgToastModule } from 'ng-angular-popup';
import { HttpClientModule } from '@angular/common/http';
import { AppRoutingModule } from '../app-routing.module';
import { BrowserModule } from '@angular/platform-browser';
// import { LandingPageComponent } from './components/landing-page/landing-page.component';


@NgModule({
  declarations: [
    DashboardComponent,
    SidebarAreaComponent,
    HeaderComponent,
    UserManagementComponent,
    ModalComponent,
    SettingsComponent,
    ProductComponent,
    RoleComponent,
    ProductTypeComponent,
    ModuleSetupComponent,
    SalesRequisitionComponent,
    RequisitionListComponent,
    SalesInvoiceComponent,
    InvoiceListComponent,
    CollectionHistoryComponent,
    RequisitionDetailsComponent,
    ProductwiseRequisitionComponent,
    TimeSettingComponent,
    SidebarAreaComponent,
    HeaderComponent,
    NoticeComponent,
    // LandingPageComponent
  ],
  imports: [
    CommonModule,
    DashboardRoutingModule,
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    NgToastModule,
    SharedModule,
    FormsModule,
    BrowserAnimationsModule
  ]
})
export class DashboardModule { }
