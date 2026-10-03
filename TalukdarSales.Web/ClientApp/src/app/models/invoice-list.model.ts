export interface SalesInvoiceDto {
// string SalesRequisitionNo
// DateTime? CreatedDateTime
// double? Quantity
// int? UserId
// string UserName
// double? TotalPrice
// double? DiscountAmount
// double? DiscountPercentage
// double? CollectionAmount

    // Define properties that match the C# class
    id : number;
    invoiceNumber: string;
    // Add all other properties as per your C# model
}

export interface SalesInvoiceInfoDto {
    salesInvoiceInfo: SalesInvoiceDto[];
    totalOfTotalPrice: number | null;
    totalCollectionAmount: number | null;
    totalDueAmount: number | null;
}