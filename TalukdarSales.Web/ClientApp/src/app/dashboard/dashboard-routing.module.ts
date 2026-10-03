import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { DashboardComponent } from './dashboard.component';
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
import { LandingPageComponent } from './components/landing-page/landing-page.component';
import { ReportsComponent } from './components/reports/reports.component';
import { DemoComponent } from './components/demo/demo.component';
import { AuthGuard } from '../guards/auth.guard';

const routes: Routes = [{
  path: '', component: DashboardComponent, canActivate: [AuthGuard],

  children: [
    // { path: '', redirectTo: 'login', pathMatch: 'full' },
    { path: '', component: LandingPageComponent,canActivate: [AuthGuard] },
    { path: 'user-management', component: UserManagementComponent,canActivate: [AuthGuard] },
    { path: 'role', component: RoleComponent,canActivate: [AuthGuard] },
    { path: 'product', component: ProductComponent,canActivate: [AuthGuard] },
    { path: 'product-type', component: ProductTypeComponent,canActivate: [AuthGuard] },
    { path: 'module-setup', component: ModuleSetupComponent,canActivate: [AuthGuard] },
    { path: 'sales-requisition', component: SalesRequisitionComponent,canActivate: [AuthGuard] },
    { path: 'requisition-list', component: RequisitionListComponent,canActivate: [AuthGuard] },
    { path: 'sales-invoice', component: SalesInvoiceComponent,canActivate: [AuthGuard] },
    { path: 'invoice-list', component: InvoiceListComponent,canActivate: [AuthGuard] },
    { path: 'collection-history/:id', component: CollectionHistoryComponent,canActivate: [AuthGuard] },
    { path: 'collection-history', component: CollectionHistoryComponent,canActivate: [AuthGuard] },
    { path: 'requisition-list/:id', component: RequisitionDetailsComponent,canActivate: [AuthGuard] },
    { path: 'productionwise-requisition', component: ProductwiseRequisitionComponent,canActivate: [AuthGuard] },
    { path: 'time-setting', component: TimeSettingComponent,canActivate: [AuthGuard] },
    { path: 'notice', component: NoticeComponent,canActivate: [AuthGuard] },
    { path: 'report', component: ReportsComponent,canActivate: [AuthGuard] },
    { path: 'demo', component: DemoComponent,canActivate: [AuthGuard] },
    { path: 'landing-page', component: LandingPageComponent,canActivate: [AuthGuard] },


  ],

}];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class DashboardRoutingModule { }
