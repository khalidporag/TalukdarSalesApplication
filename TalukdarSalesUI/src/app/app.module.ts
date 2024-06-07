import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HTTP_INTERCEPTORS, HttpClientModule } from '@angular/common/http';
import { LoginComponent } from './components/login/login.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { NgToastModule } from 'ng-angular-popup';
import { TokenInterceptor } from './interceptors/token.interceptor';
import { SharedModule } from './shared/shared.module';
import { UserManagementComponent } from './components/user-management/user-management.component';
import { ModalComponent } from './components/modal/modal.component';
import { SidebarComponent } from './components/sidebar/sidebar.component';
import { SettingsComponent } from './components/settings/settings.component';
import { ProductComponent } from './components/product/product.component';
import { RoleComponent } from './components/role/role.component';
import { ProductTypeComponent } from './components/product-type/product-type.component';
import { ModuleSetupComponent } from './components/module-setup/module-setup.component';
import { SalesRequisitionComponent } from './components/sales-requisition/sales-requisition.component';
import { RequisitionListComponent } from './components/requisition-list/requisition-list.component';
import { SalesInvoiceComponent } from './components/sales-invoice/sales-invoice.component';
import { InvoiceListComponent } from './components/invoice-list/invoice-list.component';

@NgModule({
  declarations: [
    AppComponent,
    LoginComponent,
    DashboardComponent,
    UserManagementComponent,
    ModalComponent,
    SidebarComponent,
    SettingsComponent,
    ProductComponent,
    RoleComponent,
    ProductTypeComponent,
    ModuleSetupComponent,
    SalesRequisitionComponent,
    RequisitionListComponent,
    SalesInvoiceComponent,
    InvoiceListComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    NgToastModule,
    SharedModule,
    FormsModule,
    BrowserAnimationsModule
  ],
  providers: [{
    provide:HTTP_INTERCEPTORS,
    useClass:TokenInterceptor,
    multi:true
  }],
  bootstrap: [AppComponent]
})
export class AppModule { }
