export interface SalesInvoiceDetailsDto {
  id: number;
  salesInvoiceId?: number;
  finishedGoodsId?: number;
  createdDateTime?: Date;
  quantity?: number;
  price?: number;
  discountAmount?: number;
  discountPercentage?: number;
  finishGoodName?: string;
}