import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';

@Component({
  selector: 'app-product-type',
  templateUrl: './product-type.component.html',
  styleUrls: ['./product-type.component.scss']
})
export class ProductTypeComponent implements OnInit {
  public productTypes:any = [];
  public userTypes:any = [];
  public createProductTypeForm!: FormGroup;
  createModal: boolean = false;
  submitting: boolean = false;
  bodyText = 'This text can be updated in modal 1';

  nameSearch: string = '';

  constructor(
    private api : ApiService,
    private modalService1: NzModalService,
    protected modalService: ModalService,
    private auth: AuthService,
    private toast: NgToastService,
    private fb: FormBuilder,
  ){}

  ngOnInit(): void {
    this.getProductTypes();
  }

  getProductTypes(){
    this.api.getFinishGoodTypes(this.nameSearch)
    .subscribe(res=>{
    this.productTypes = res;
    });
  }

  getUserTypes(){
    this.api.getUserTypes()
    .subscribe(res=>{
    this.userTypes = res;
    console.log(this.userTypes)
    });
  }

  private createInit(): void {
    this.createProductTypeForm = this.fb.group({
      name:['', Validators.required]
    });
  }

  openTypeModal() {
    this.createModal = true;
    this.submitting = false;
    this.getUserTypes();
    this.createInit();
  }

  closeModal(){
    this.createModal = false;
    this.submitting = false;
  }

  onSubmit(){
    if (this.createProductTypeForm.valid) {
      this.submitting = true;
      this.api.createFinishedGoodType(this.createProductTypeForm.value).subscribe({
        next: (res) => {
          this.createProductTypeForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getProductTypes();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createProductTypeForm);
    }
  }
}
