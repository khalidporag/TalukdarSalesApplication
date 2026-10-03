import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ConfigService {
  rootURL: string = "JD";
  baseURL: string = "";
  imageURL: string = "";
  baseDomain: string = "";
  domain: string = "";
  short_name: string = "";
  serverType: string = "";
  version: string = "";

  public configUrl: string = "/assets/config/config.json";

  constructor(private httpClient: HttpClient) { }

  public getConfigFile() {
    let promise = this.httpClient.get(this.configUrl).toPromise().then((data:any)=>{
      this.setIntoVariable(data);
    })
    return promise;
  }

  setIntoVariable(res:any) {
    this.baseDomain = res?.bdom;
    this.imageURL = res?.idom;
    this.short_name = res?.short_name;
    this.serverType = res?.serverType;
    this.version = res?.version;
  }
}
