import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { NgToastService } from 'ng-angular-popup';
import { NzModalService } from 'ng-zorro-antd/modal';
import ValidateForm from 'src/app/helpers/validationform';
import { SalesInvoiceDetailsDto } from 'src/app/models/SalesInvoiceDetailsDto.model';
import { ApiService } from 'src/app/services/api.service';
import { AuthService } from 'src/app/services/auth.service';
import { ModalService } from 'src/app/services/modal.service';
import { environment } from 'src/environments/environment';
import * as XLSX from 'xlsx';  // Import xlsx

@Component({
  selector: 'app-invoice-list',
  templateUrl: './invoice-list.component.html',
  styleUrls: ['./invoice-list.component.scss']
})
export class InvoiceListComponent implements OnInit {

  public collectionHistory:any = [];
  invoiceListDateRange: Date[] = [];

  dateFormat = 'yyyy/MM/dd';
  monthFormat = 'yyyy/MM';
  quarterFormat = 'yyyy/[Q]Q';

  selectedUserGroup: any;
  selectedStatus: any;
  selectedUser: any;
  requisitionNo: string = '';
  selectedInvoiceId: any;
  public productTypes:any = [];
  public invoiceList:any = [];
  public userTypes:any = [];
  public users:any = [];
  public createCollectionForm!: FormGroup;
  createCollectionModal: boolean = false;
  submitting: boolean = false;
  bodyText = 'This text can be updated in modal 1';

  

  baseUrl = environment.apiBaseUrl;

  constructor(
    private api : ApiService,
    private modalService1: NzModalService,
    protected modalService: ModalService,
    private auth: AuthService,
    private toast: NgToastService,
    private fb: FormBuilder,
    private router: Router
  ){}

  ngOnInit(): void {
    const today = new Date(); // Get today's date
    this.invoiceListDateRange = [today, today]; 
    this.getProductTypes();
    this.getInvoiceList();
    this.getUsers();
    this.getUserTypes();
  }

  onDateRangeChangeForInvoiceList(result: Date[]): void {
    this.invoiceListDateRange = result;
    this.getInvoiceList();
  }

  exportTableToExcel() {
    // Process the invoice data and remove the ID column
    const dataToExport = this.invoiceList.salesInvoiceInfo.map((invoice: any) => ({
      InvoiceFor: invoice.userName,
      InvoiceNo: invoice.invoiceNumber,
      RequisitionNo: invoice.salesRequisitionNo,
      CreationTime: new Date(invoice.createdDateTime).toLocaleString(), // Format the date
      Total: this.formatNumberWithCommas(invoice.totalPrice),
      Collection: this.formatNumberWithCommas(invoice.collectionAmount),
      Due: this.formatNumberWithCommas(invoice.totalPrice - invoice.collectionAmount)
    }));

    // Create a worksheet from the processed data
    const worksheet = XLSX.utils.json_to_sheet(dataToExport);

    // Create a new workbook and append the worksheet
    const workbook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(workbook, worksheet, 'Invoice Data');

    // Save the workbook as an Excel file
    XLSX.writeFile(workbook, 'InvoiceList.xlsx');
  }


  exportSingleInvoiceToExcel(invoiceId: string) {
    // Fetch the detailed invoice information using the invoice ID
    this.api.getInvoiceDetails(invoiceId).subscribe(invoiceDetails => {

      debugger;

        // Prepare the main invoice data
        const invoiceData = [
            ["Talukdar Foods"], // Title
            [""], // Empty row
            ["Invoice", invoiceDetails.invoiceNumber],
            ["Name", `${invoiceDetails.userName}`],
            ["User ID", `${invoiceDetails.userSequencialId}`],
            ["Date", new Date(invoiceDetails.createdDateTime!).toLocaleString()],
            [""], // Empty row before details
            ["Details"] // Details section title
        ];
  
        // Prepare the sales invoice details with explicit typing for `detail`
        const salesInvoiceDetailsExport = invoiceDetails.salesInvoiceDetails.map((detail: SalesInvoiceDetailsDto, index: number) => [
            index + 1,  // SL
            detail.finishGoodName,  // Product Name
            this.formatNumberWithCommas(detail.price || 0),  // Unit Price
            detail.quantity || 0,  // Quantity
            this.formatNumberWithCommas((detail.price || 0) * (detail.quantity || 0))  // Total
        ]);
  
        // Header for the invoice details table
        const detailsHeader = ["SL", "Product Name", "Unit Price", "Quantity", "Total"];
  
        // Calculate the total quantity and total price for the summary row
        const totalQuantity = invoiceDetails.quantity;
        const totalPrice = invoiceDetails.totalPrice;
  
        // Invoice total row
        const invoiceTotalRow = [
            "Invoice Total",  // Label
            "",  // Empty cell
            "",  // Empty cell
            totalQuantity,  // Total Quantity
            this.formatNumberWithCommas(totalPrice)  // Total Price
        ];
  
        // Combine the invoice data, details header, details, and total row into a single array
        const finalData = [
            ...invoiceData,  // Invoice header and meta
            detailsHeader,  // Table header
            ...salesInvoiceDetailsExport,  // Table rows (invoice details)
            invoiceTotalRow  // Total row
        ];
  
        // Create a new worksheet from the combined data
        const worksheet = XLSX.utils.aoa_to_sheet(finalData);
  

    //   // Set fixed column widths (in pixels or "characters" by default)
    //   worksheet['!cols'] = [
    //     { wpx: 30 },   // Column 1: SL (e.g., 30 pixels wide)
    //     { wpx: 200 },  // Column 2: Product Name (e.g., 200 pixels wide)
    //     { wpx: 80 },   // Column 3: Unit Price (e.g., 80 pixels wide)
    //     { wpx: 80 },   // Column 4: Quantity (e.g., 80 pixels wide)
    //     { wpx: 100 }   // Column 5: Total (e.g., 100 pixels wide)
    // ];


    // Define a cell style for center alignment and border
    const centerAlignmentWithBorderStyle = {
      alignment: { horizontal: 'center' },
      border: {
        top: { style: 'thin' },
        bottom: { style: 'thin' },
        left: { style: 'thin' },
        right: { style: 'thin' }
      }
    };

    // Apply the style to the header row (SL, Product Name, Unit Price, Quantity, Total)
    const headerRowIndex = invoiceData.length; // The row where the headers start

    detailsHeader.forEach((_, colIndex) => {
      const cellRef = XLSX.utils.encode_cell({ r: headerRowIndex, c: colIndex });
      if (!worksheet[cellRef]) worksheet[cellRef] = {};
      worksheet[cellRef].s = centerAlignmentWithBorderStyle;
    });

    // Apply center alignment and border to each cell in the details rows (invoice details)
    for (let rowIndex = headerRowIndex + 1; rowIndex < finalData.length; rowIndex++) {
      for (let colIndex = 0; colIndex < 5; colIndex++) {  // SL to Total (5 columns)
        const cellRef = XLSX.utils.encode_cell({ r: rowIndex, c: colIndex });
        if (!worksheet[cellRef]) worksheet[cellRef] = {};
        worksheet[cellRef].s = centerAlignmentWithBorderStyle;
      }
    }



        // Create a new workbook and append the worksheet
        const workbook = XLSX.utils.book_new();
        XLSX.utils.book_append_sheet(workbook, worksheet, 'Invoice Details');
  
        // Save the workbook as an Excel file
        XLSX.writeFile(workbook, `Invoice_${invoiceId}.xlsx`);
    }, error => {
        this.toast.error({ detail: "ERROR", summary: "Failed to fetch invoice details!", duration: 5000 });
    });
  }
  


//   exportSingleInvoiceToExcel(invoiceId: string) {
//     // Fetch the detailed invoice information using the invoice ID
//     this.api.getInvoiceDetails(invoiceId).subscribe(invoiceDetails => {
//         // Process the detailed invoice data to prepare for export

//         console.log(invoiceDetails);
//         debugger;


//         const invoiceData = [
//           { title: "Invoice", value: invoiceDetails.invoiceNumber },
//           { title: "Name", value: `${invoiceDetails.userName} (${invoiceDetails.userSequencialId})` },
//           { title: "Date", value: new Date(invoiceDetails.createdDateTime!).toLocaleString() }
//       ];

//        // Prepare the sales invoice details with explicit typing for `detail`
//       const salesInvoiceDetailsExport = invoiceDetails.salesInvoiceDetails.map((detail: SalesInvoiceDetailsDto, index: number) => ({
//         SL: index + 1,
//         ProductName: detail.finishGoodName,
//         UnitPrice: this.formatNumberWithCommas(detail.price || 0),
//         Quantity: detail.quantity || 0,
//         Total: this.formatNumberWithCommas((detail.price || 0) * (detail.quantity || 0)),
//     }));
    
//     // Create a new workbook and worksheets, similar to what you already have
//     const workbook = XLSX.utils.book_new();
//     const invoiceWorksheet = XLSX.utils.aoa_to_sheet([
//         ["Talukdar Foods"],
//         ["", "", ""],
//         ...invoiceData.map(item => [item.title, item.value]),
//         ["", "", ""],
//         ["Details"]
//     ]);

//      // Create worksheet for sales invoice details
//     const detailsWorksheet = XLSX.utils.json_to_sheet(salesInvoiceDetailsExport);

//   // Append header to the details worksheet
//   XLSX.utils.sheet_add_aoa(detailsWorksheet, [["SL", "Product Name", "Unit Price", "Quantity", "Total"]], { origin: "A1" });

//   // Set up total row
//   // const totalQuantity = salesInvoiceDetailsExport.reduce((sum, item) => sum + (item.Quantity || 0), 0);
//   // const totalPrice = salesInvoiceDetailsExport.reduce((sum, item) => sum + parseFloat(item.Total.replace(/,/g, '')), 0);

//   const totalQuantity = 100;
//   const totalPrice = 100;

//   const invoiceTotalRow = [
//       "Invoice Total",
//       "",
//       "",
//       totalQuantity,  // Total Quantity
//       this.formatNumberWithCommas(totalPrice) // Total Price
//   ];
//   XLSX.utils.sheet_add_aoa(detailsWorksheet, [invoiceTotalRow], { origin: -1 }); // Append at the end

//   // Append both worksheets to the workbook
//   XLSX.utils.book_append_sheet(workbook, invoiceWorksheet, 'Invoice Details');
//   XLSX.utils.book_append_sheet(workbook, detailsWorksheet, 'Sales Invoice Details');

//   // Save the workbook as an Excel file
//   XLSX.writeFile(workbook, `Invoice_${invoiceId}.xlsx`);
// }, error => {
//   this.toast.error({ detail: "ERROR", summary: "Failed to fetch invoice details!", duration: 5000 });


//     });
// }





  getInvoiceList() {
    this.api.getSalesInvoiceList(this.selectedUser, this.invoiceListDateRange[0], this.invoiceListDateRange[1])
    .subscribe(res => {
      this.invoiceList = res;
    });
  }

  showPdf(invoiceNumber: string){
    let pdfUrl = `{{baseUrl}}/pdf/invoices/Invoice_${invoiceNumber}.pdf`;
    window.open(pdfUrl, '_blank');
  }
  

  onFilterChange() {
    this.getInvoiceList();
  }

  getProductTypes(){
    this.api.getFinishGoodTypes()
    .subscribe(res=>{
    this.productTypes = res;
    });
  }

  getUsers(){
    this.api.getUsers()
    .subscribe(res=>{
    this.users = res;
    });
  }
  getUserTypes(){
    this.api.getUserTypes()
    .subscribe(res=>{
    this.userTypes = res;
    });
  }

  onUserGroupChange() {
    this.api.getUsersByType(this.selectedUserGroup)
    .subscribe(res=>{
    this.users = res;
    });
  }

  onCreateNew(): void {
    this.router.navigate(['/sales-requisition']);
  }

  openCollectionModal(id: any) {
    this.selectedInvoiceId = id;
    this.createCollectionModal = true;
    this.submitting = false;
    this.createInit();
  }

    private createInit(): void {
    this.createCollectionForm = this.fb.group({
      collectionAmount:[null, Validators.required],
      paymentMethod:['', Validators.required]
    });
  }

  closeModal(){
    this.createCollectionModal = false;
    this.submitting = false;
  }

  getCollectionList(id: any){
    this.router.navigate(['/collection-history', id]);
  }

  onSubmit(){
    if (this.createCollectionForm.valid) {
      this.submitting = true;
      var collectionData = {
        salesInvoiceId: this.selectedInvoiceId,
        collectionAmount: this.createCollectionForm.value.collectionAmount,
        paymentMethod: this.createCollectionForm.value.paymentMethod
      }
      this.api.collectInvoiceAmount(collectionData).subscribe({
        next: (res) => {
          this.createCollectionForm.reset();
          this.toast.success({detail:"SUCCESS", summary:res.message, duration: 5000});
          this.closeModal();
          this.getInvoiceList();
        },
        error: (err) => {
          this.toast.error({detail:"ERROR", summary:"Something when wrong!", duration: 5000});
          this.closeModal();
        },
      });
    } else {
      ValidateForm.validateAllFormFields(this.createCollectionForm);
    }
  }

  reset()
  {
    // this.invoiceListDateRange = [];
    // this.selectedUser = null;
    // this.getInvoiceList();
    window.location.reload();
  }

  formatNumberWithCommas(value: number): string {
    return value.toLocaleString('en-US');
  }
}
