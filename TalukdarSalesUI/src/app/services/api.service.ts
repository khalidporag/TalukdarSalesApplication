import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private baseUrl: string = 'https://localhost:7019/api/User/';
  private roleUrl: string = 'https://localhost:7019/api/Role/';
  private finishedGoodUrl: string = 'https://localhost:7019/api/FinishedGood/';
  private SalesRequisitionUrl: string = 'https://localhost:7019/api/SalesRequisition/';
  private SalesInvoiceUrl: string = 'https://localhost:7019/api/SalesInvoice/';
  private NoticeUrl: string = 'https://localhost:7019/api/Notice/';




  constructor(private http: HttpClient) {}

  getUsers() {
    return this.http.get<any>(this.baseUrl);
  }

  getUsersByType(userTypeId: number) {
    return this.http.get<any>(`${this.baseUrl}?userTypeId=${userTypeId}`);
  }

  getUserTypes() {
    return this.http.get<any>(`${this.baseUrl}getAllUserTypes`);
  }

  userType(loginObj : any){
    return this.http.post<any>(`${this.baseUrl}userType`,loginObj)
  }

  createRole(obj: any) {
    return this.http.post<any>(`${this.roleUrl}createRole`, obj)
  }

  addRoleWiseModule(obj: any) {
    return this.http.post<any>(`${this.roleUrl}addRoleWiseModule`, obj)
  }

  getRoles() {
    return this.http.get<any>(`${this.roleUrl}getAllRoles`);
  }

  createModule(obj: any) {
    return this.http.post<any>(`${this.roleUrl}createModule`, obj)
  }

  getAllModules() {
    return this.http.get<any>(`${this.roleUrl}getAllModules`);
  }

  createFinishedGood(obj: any) {
    return this.http.post<any>(`${this.finishedGoodUrl}createFinishedGood`, obj)
  }

  updateFinishedGood(obj: any) {
    return this.http.post<any>(`${this.finishedGoodUrl}updateFinishedGood`, obj)
  }

  getallFinishedGoods(param1?: number, param2?: string) {
    let params: any = {};
  
    if (param1 !== undefined) params.goodTypeId = param1;
    if (param2 !== undefined && param2 !== null) params.finishedGoodName = param2;
  
    const queryString = new URLSearchParams(params).toString();
    return this.http.get<any>(`${this.finishedGoodUrl}getAllFinishedGoods?${queryString}`);
  }

  getallFinishedGoodsByType(goodTypeId: number) {
    return this.http.get<any>(`${this.finishedGoodUrl}getAllFinishedGoods?goodTypeId=${goodTypeId}`);
  }

  createFinishedGoodType(obj: any) {
    return this.http.post<any>(`${this.finishedGoodUrl}FinishGoodType`, obj)
  }

  getFinishGoodTypes() {
    return this.http.get<any>(`${this.finishedGoodUrl}getAllFinishGoodTypes`);
  }

  createSalesRequisitionWithDetails(obj: any) {
    return this.http.post<any>(`${this.SalesRequisitionUrl}createSalesRequisitionWithDetail`, obj)
  }

  getSalesRequisitionList(param1?: boolean, param2?: number, param3?: string) {
    let params: any = {};
  
    if (param1 !== undefined) params.isActive = param1;
    if (param2 !== undefined) params.userId = param2;
    if (param3 !== undefined && param3 !== null) params.requisitionNo = param3;
  
    const queryString = new URLSearchParams(params).toString();
  
    return this.http.get<any>(`${this.SalesRequisitionUrl}getSalesRequisitionList?${queryString}`);
  }

  getSalesRequisitionDetailsList(param1?: number) {
    let params: any = {};
    if (param1 !== undefined) params.requisitionId = param1;
    const queryString = new URLSearchParams(params).toString();
    return this.http.get<any>(`${this.SalesRequisitionUrl}getSalesRequisitionDetailsList?${queryString}`);
  }

  updateTimeSetting(obj: any) {
    return this.http.post<any>(`${this.SalesRequisitionUrl}updateTimeSetting`, obj)
  }
  
  getTimeSetting(){
    return this.http.get<any>(`${this.SalesRequisitionUrl}getTimeSetting`);
  }

  getProductWiseDailyRequisition()
  {
    return this.http.get<any>(`${this.SalesRequisitionUrl}getProductWiseDailyRequisition`);
  }

  createBulkInvoiceWithDetails(obj: any) {
    return this.http.post<any>(`${this.SalesInvoiceUrl}createBulkInvoiceWithDetails`, obj);
  }

  createSalesInvoiceWithDetails(obj: any) {
    return this.http.post<any>(`${this.SalesInvoiceUrl}createSalesInvoiceWithDetails`, obj)
  }

  collectInvoiceAmount(obj: any) {
    return this.http.post<any>(`${this.SalesInvoiceUrl}collectInvoiceAmount`, obj)
  }

  getSalesInvoiceList(param1?: number) {
    let params: any = {};
    if (param1 !== undefined) params.userId = param1;
    const queryString = new URLSearchParams(params).toString();
    return this.http.get<any>(`${this.SalesInvoiceUrl}getSalesInvoiceList?${queryString}`);
  }

  getCollectionHistory(param1?: number, param2?: number) {
    let params: any = {};
    if (param1 !== undefined) params.userId = param1;
    if (param2 !== undefined) params.salesInvoiceId = param2;
    const queryString = new URLSearchParams(params).toString();
    return this.http.get<any>(`${this.SalesInvoiceUrl}getCollectionHistory?${queryString}`);
  }
  
  getTopFiveSeller(fromDate?: string, toDate?: string): Observable<any[]> {
    let params = new HttpParams();
    if (fromDate) {
      params = params.set('from', fromDate);
    }
    if (toDate) {
      params = params.set('to', toDate);
    }
    // return this.http.get<any[]>('your-api-url/top-sellers', { params });
    return this.http.get<any>(`${this.SalesInvoiceUrl}getTopFiveSeller`, {params});

  }

  // getTopFiveSeller()
  // {
  //   return this.http.get<any>(`${this.SalesInvoiceUrl}getTopFiveSeller`);
  // }

  getTopFiveSellingProduct()
  {
    return this.http.get<any>(`${this.SalesInvoiceUrl}getTopFiveSellingProduct`);
  }

  getLessFiveSellingProduct()
  {
    return this.http.get<any>(`${this.SalesInvoiceUrl}getLessFiveSellingProduct`);
  }

  getTopFiveSellerWithDueAmount()
  {
    return this.http.get<any>(`${this.SalesInvoiceUrl}getTopFiveSellerWithDueAmount`);
  }

  getDailyAccumulatedOrderSummary()
  {
    return this.http.get<any>(`${this.SalesRequisitionUrl}getDailyAccumulatedOrderSummary`);
  }

  createNotice(obj: any) {
    return this.http.post<any>(`${this.NoticeUrl}createNotice`, obj)
  }

  getAllNotices() {
    return this.http.get<any>(`${this.NoticeUrl}getAllNotices`);
  }
  
}
