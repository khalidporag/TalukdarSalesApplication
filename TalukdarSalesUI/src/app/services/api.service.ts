import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';



@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private userUrl: string = `${environment.apiBaseUrl}/api/User/`;
  private roleUrl: string = `${environment.apiBaseUrl}/api/Role/`;
  private finishedGoodUrl: string = `${environment.apiBaseUrl}/api/FinishedGood/`;
  private SalesRequisitionUrl: string = `${environment.apiBaseUrl}/api/SalesRequisition/`;
  private SalesInvoiceUrl: string = `${environment.apiBaseUrl}/api/SalesInvoice/`;
  private NoticeUrl: string = `${environment.apiBaseUrl}/api/Notice/`;


  constructor(private http: HttpClient) {}

  updateUser(userObj: any) {
    return this.http.post<any>(`${this.userUrl}updateUser`, userObj)
  }

  getUsers(param1?: number, param3?: string) {
    let params: any = {};
    if (param1 !== undefined) params.userTypeId = param1;
    if (param3 !== undefined && param3 !== null) params.name = param3;

    const queryString = new URLSearchParams(params).toString();
    return this.http.get<any>(`${this.userUrl}?${queryString}`);
  }

  getUsersByType(userTypeId: number) {
    return this.http.get<any>(`${this.userUrl}?userTypeId=${userTypeId}`);
  }

  getUserTypes() {
    return this.http.get<any>(`${this.userUrl}getAllUserTypes`);
  }

  userType(loginObj : any){
    return this.http.post<any>(`${this.userUrl}userType`,loginObj)
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

  getFinishGoodTypes(param3?: string) {
    let params: any = {};
    if (param3 !== undefined && param3 !== null) params.name = param3;
    const queryString = new URLSearchParams(params).toString();
    return this.http.get<any>(`${this.finishedGoodUrl}getAllFinishGoodTypes?${queryString}`);
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
  
  getTopFiveSeller(fromDate?: Date, toDate?: Date): Observable<any[]> {
    let params = new HttpParams();
    if (fromDate) {
      params = params.set('from', fromDate.toISOString().split('T')[0]);
    }
    if (toDate) {
      params = params.set('to', toDate.toISOString().split('T')[0]);
    }
    return this.http.get<any>(`${this.SalesInvoiceUrl}getTopFiveSeller`, {params});

  }

  getTopFiveSellingProduct(fromDate?: Date, toDate?: Date): Observable<any[]> {
    let params = new HttpParams();
    if (fromDate) {
      params = params.set('from', fromDate.toISOString().split('T')[0]);
    }
    if (toDate) {
      params = params.set('to', toDate.toISOString().split('T')[0]);
    }
    return this.http.get<any>(`${this.SalesInvoiceUrl}getTopFiveSellingProduct`, {params});
  }

  // getTopFiveSellingProduct()
  // {
  //   return this.http.get<any>(`${this.SalesInvoiceUrl}getTopFiveSellingProduct`);
  // }

  getLessFiveSellingProduct(fromDate?: Date, toDate?: Date): Observable<any[]> {
    let params = new HttpParams();
    if (fromDate) {
      params = params.set('from', fromDate.toISOString().split('T')[0]);
    }
    if (toDate) {
      params = params.set('to', toDate.toISOString().split('T')[0]);
    }
    return this.http.get<any>(`${this.SalesInvoiceUrl}getLessFiveSellingProduct`, {params});
  }

  // getLessFiveSellingProduct()
  // {
  //   return this.http.get<any>(`${this.SalesInvoiceUrl}getLessFiveSellingProduct`);
  // }

  getTopFiveSellerWithDueAmount(fromDate?: Date, toDate?: Date): Observable<any[]> {
    let params = new HttpParams();
    if (fromDate) {
      params = params.set('from', fromDate.toISOString().split('T')[0]);
    }
    if (toDate) {
      params = params.set('to', toDate.toISOString().split('T')[0]);
    }
    return this.http.get<any>(`${this.SalesInvoiceUrl}getTopFiveSellerWithDueAmount`, {params});
  }

  // getTopFiveSellerWithDueAmount()
  // {
  //   return this.http.get<any>(`${this.SalesInvoiceUrl}getTopFiveSellerWithDueAmount`);
  // }

  getDailyAccumulatedOrderSummary(fromDate?: Date, toDate?: Date): Observable<any[]> {
    let params = new HttpParams();
    if (fromDate) {
      params = params.set('from', fromDate.toISOString().split('T')[0]);
    }
    if (toDate) {
      params = params.set('to', toDate.toISOString().split('T')[0]);
    }
    return this.http.get<any>(`${this.SalesInvoiceUrl}getDailyAccumulatedSalesSummary`, {params});
  }

  // getDailyAccumulatedOrderSummary()
  // {
  //   return this.http.get<any>(`${this.SalesRequisitionUrl}getDailyAccumulatedOrderSummary`);
  // }

  createNotice(obj: any) {
    return this.http.post<any>(`${this.NoticeUrl}createNotice`, obj)
  }

  getAllNotices(param1?: boolean) {
    let params: any = {};
  
    if (param1 !== undefined) params.isLanding = param1;
  
    const queryString = new URLSearchParams(params).toString();

    return this.http.get<any>(`${this.NoticeUrl}getAllNotices?${queryString}`);
  }
  
}
