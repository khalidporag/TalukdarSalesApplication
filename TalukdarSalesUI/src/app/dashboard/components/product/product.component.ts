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

  file: File | null = null;
  fileError: string = '';

  public products:any = [];
  public finishedGoodTypes:any = [];
  public createFinishedGoodForm!: FormGroup;
  public editFinishedGoodForm!: FormGroup;
  createModal: boolean = false;
  editModal: boolean = false;
  selectedFinishedGood: any;
  selectedStatus: any;
  selectedType: any;
  productName: any;

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
    this.getFinishGoodTypes();
  }

  onFileChange(event: any): void {
    const file = event.target.files[0];
    if (file) {
      this.file = file;
      this.fileError = '';
    } else {
      this.fileError = 'Please select an image file.';
    }
  }

  getFinishedGoods(){
    this.api.getallFinishedGoods(this.selectedType, this.productName)
    .subscribe(res=>{
    this.products = res;
    console.log(res);
    });
  }

  onFilterChange() {
    this.getFinishedGoods();
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
      unitPrice:['', Validators.required],
      image: [null, Validators.nullValidator]
    });
  }

  private editInit(): void {
    this.editFinishedGoodForm = this.fb.group({
      description:[this.selectedFinishedGood.description || '', Validators.required],
      unitPrice:[this.selectedFinishedGood.unitPrice || '', Validators.required],
      isActive:[this.selectedFinishedGood.isActive || null, Validators.required]
    });
  }

  openFinishedGoodModal() {
    // this.modalService.open('modal-1');
    this.createModal = true;
    this.submitting = false;
    this.getFinishGoodTypes();
    this.createInit();
  }

  openEditFinishedGoodModal(obj: any) {
    // this.modalService.open('modal-1');
    this.selectedFinishedGood = obj;
    this.editModal = true;
    this.submitting = false;
    this.editInit();
  }

  closeModal(){
    this.createModal = false;
    this.editModal = false;
    this.submitting = false;
  }

  onSubmit(){
    if (this.createFinishedGoodForm.valid) {
      this.submitting = true;

    const formData = new FormData();
    formData.append('name', this.createFinishedGoodForm.value.name);
    formData.append('uOM', this.createFinishedGoodForm.value.uOM);
    formData.append('goodTypeId', this.createFinishedGoodForm.value.finishedGoodTypeId);
    formData.append('description', this.createFinishedGoodForm.value.description);
    formData.append('unitPrice', this.createFinishedGoodForm.value.unitPrice);

    // Append the image file to the FormData object if it exists
    if (this.file) {
      formData.append('image', this.file, this.file.name);
    }

      this.api.createFinishedGood(formData).subscribe({
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

  editProduct(){
    if (this.editFinishedGoodForm.valid) {
      this.submitting = true;
      let product = {
        id: this.selectedFinishedGood.id,
        description: this.editFinishedGoodForm.value.description,
        unitPrice: this.editFinishedGoodForm.value.unitPrice,
        isActive: this.editFinishedGoodForm.value.isActive
      }
      this.api.updateFinishedGood(product).subscribe({
        next: (res) => {
          this.editFinishedGoodForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getFinishedGoods();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
          console.log(err);
          this.closeModal();
          this.getFinishedGoods();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.editFinishedGoodForm);
    }
  }
}
