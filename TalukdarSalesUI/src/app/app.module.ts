import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
// import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
// import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
// import { NgToastModule } from 'ng-angular-popup';
import { TokenInterceptor } from './interceptors/token.interceptor';
// import { SharedModule } from './shared/shared.module';
// import { LoginComponent } from './auth/components/login/login.component';
import { DashboardModule } from './dashboard/dashboard.module';
import { AuthModule } from './auth/auth.module';
// import { LandingPageComponent } from './dashboard/components/landing-page/landing-page.component';
// import { LoginComponent } from './dashboard/components/login/login.component';
// import { DashboardComponent } from './dashboard/components/dashboard/dashboard.component';
// import { UserManagementComponent } from './dashboard/components/user-management/user-management.component';
// import { ModalComponent } from './dashboard/components/modal/modal.component';
// import { SidebarComponent } from './dashboard/components/sidebar/sidebar.component';
// import { SettingsComponent } from './dashboard/components/settings/settings.component';
// import { ProductComponent } from './dashboard/components/product/product.component';
// import { RoleComponent } from './dashboard/components/role/role.component';
// import { ProductTypeComponent } from './dashboard/components/product-type/product-type.component';
// import { ModuleSetupComponent } from './dashboard/components/module-setup/module-setup.component';
// import { SalesRequisitionComponent } from './dashboard/components/sales-requisition/sales-requisition.component';
// import { RequisitionListComponent } from './dashboard/components/requisition-list/requisition-list.component';
// import { SalesInvoiceComponent } from './dashboard/components/sales-invoice/sales-invoice.component';
// import { InvoiceListComponent } from './dashboard/components/invoice-list/invoice-list.component';
// import { CollectionHistoryComponent } from './dashboard/components/collection-history/collection-history.component';
// import { RequisitionDetailsComponent } from './dashboard/components/requisition-details/requisition-details.component';
// import { NoticeComponent } from './dashboard/components/notice/notice.component';
// import { ProductwiseRequisitionComponent } from './dashboard/components/productwise-requisition/productwise-requisition.component';
// import { TimeSettingComponent } from './dashboard/components/time-setting/time-setting.component';
// import { SidebarAreaComponent } from './sidebar-area/sidebar-area.component';
// import { HeaderComponent } from './header/header.component';


@NgModule({
  declarations: [
    AppComponent,
    // LandingPageComponent,
    // LoginComponent

    // LoginComponent,
    // DashboardComponent,
    // UserManagementComponent,
    // ModalComponent,
    // SidebarComponent,
    // SettingsComponent,
    // ProductComponent,
    // RoleComponent,
    // ProductTypeComponent,
    // ModuleSetupComponent,
    // SalesRequisitionComponent,
    // RequisitionListComponent,
    // SalesInvoiceComponent,
    // InvoiceListComponent,
    // CollectionHistoryComponent,
    // RequisitionDetailsComponent,
    // ProductwiseRequisitionComponent,
    // TimeSettingComponent,
    // SidebarAreaComponent,
    // HeaderComponent,
    // NoticeComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    DashboardModule,
    AuthModule
    // NgToastModule,
    // SharedModule,
    // FormsModule,
    // BrowserAnimationsModule,
    // QuillModule.forRoot()
  ],
  providers: [{
    provide: HTTP_INTERCEPTORS,
    useClass: TokenInterceptor,
    multi: true
  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
