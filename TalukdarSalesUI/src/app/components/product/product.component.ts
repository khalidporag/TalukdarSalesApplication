import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';

@Component({
  selector: 'app-product',
  templateUrl: './product.component.html',
  styleUrls: ['./product.component.scss']
})
export class ProductComponent implements OnInit {
  public products:any = [];
  public finishedGoodTypes:any = [];
  public createFinishedGoodForm!: FormGroup;
  createModal: boolean = false;
  submitting: boolean = false;
  bodyText = 'This text can be updated in modal 1';
  selectedProductType: any;
  constructor(
    private api : ApiService,
    private modalService1: NzModalService,
    protected modalService: ModalService,
    private auth: AuthService,
    private toast: NgToastService,
    private fb: FormBuilder,
  ){}

  ngOnInit(): void {
    this.getFinishedGoods();
  }

  getFinishedGoods(){
    this.api.getallFinishedGoods()
    .subscribe(res=>{
    this.products = res;
    console.log(res);
    });
  }

  getFinishGoodTypes(){
    this.api.getFinishGoodTypes()
    .subscribe(res=>{
    this.finishedGoodTypes = res;
    console.log(this.finishedGoodTypes)
    });
  }

  private createInit(): void {
    this.createFinishedGoodForm = this.fb.group({
      name:['', Validators.required],
      uOM:['', Validators.required],
      finishedGoodTypeId:[null, Validators.required],
      description:['', Validators.required],
      unitPrice:['', Validators.required]
    });
  }

  openFinishedGoodModal() {
    // this.modalService.open('modal-1');
    this.createModal = true;
    this.submitting = false;
    this.getFinishGoodTypes();
    this.createInit();
  }

  closeModal(){
    this.createModal = false;
    this.submitting = false;
  }

  onSubmit(){
    if (this.createFinishedGoodForm.valid) {
      this.submitting = true;
      let product = {
        name: this.createFinishedGoodForm.value.name,
        uom: this.createFinishedGoodForm.value.uOM,
        goodTypeId: this.selectedProductType,
        description: this.createFinishedGoodForm.value.description,
        unitPrice: this.createFinishedGoodForm.value.unitPrice
      }
      this.api.createFinishedGood(product).subscribe({
        next: (res) => {
          this.createFinishedGoodForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getFinishedGoods();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
          console.log(err);
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createFinishedGoodForm);
    }
  }
}
