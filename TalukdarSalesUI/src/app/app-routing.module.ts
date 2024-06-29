import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { SignupComponent } from './components/signup/signup.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { UserManagementComponent } from './components/user-management/user-management.component';
import { RoleComponent } from './components/role/role.component';
import { ProductComponent } from './components/product/product.component';
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

const routes: Routes = [
  {path:'', redirectTo:'login', pathMatch:'full'},
  {path: 'dashboard', component: DashboardComponent},
  {path: 'login', component: LoginComponent},
  {path: 'signup', component: SignupComponent},
  {path: 'user-management', component: UserManagementComponent},
  {path: 'role', component: RoleComponent},
  {path: 'product', component: ProductComponent},
  {path: 'product-type', component: ProductTypeComponent},
  {path: 'module-setup', component: ModuleSetupComponent},
  {path: 'sales-requisition', component: SalesRequisitionComponent},
  {path: 'requisition-list', component: RequisitionListComponent},
  {path: 'sales-invoice', component: SalesInvoiceComponent},
  {path: 'invoice-list', component: InvoiceListComponent},
  {path: 'collection-history/:id', component: CollectionHistoryComponent},
  {path: 'collection-history', component: CollectionHistoryComponent},
  {path: 'requisition-list/:id', component: RequisitionDetailsComponent},
  {path: 'productionwise-requisition', component: ProductwiseRequisitionComponent},
  {path: 'time-setting', component: TimeSettingComponent},
  {path: 'notice', component: NoticeComponent},



];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
