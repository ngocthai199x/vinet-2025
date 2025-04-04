import {
  MatListOption,
  MatSelectionList,
  MatSelectionListChange,
} from '@angular/material/list';
import { Component, inject, OnInit } from '@angular/core';
import { ShopService } from '../../core/services/shop.service';
import { Product } from '../../shared/models/product';
import { ProductItemComponent } from './product-item/product-item.component';
import { MatDialog } from '@angular/material/dialog';
import { FiltersDialogComponent } from './filters-dialog/filters-dialog.component';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatMenu, MatMenuTrigger } from '@angular/material/menu';

@Component({
  selector: 'app-shop',
  standalone: true,
  imports: [
    ProductItemComponent,
    MatButton,
    MatIcon,
    MatMenu,
    MatSelectionList,
    MatListOption,
    MatMenuTrigger,
  ],
  templateUrl: './shop.component.html',
  styleUrl: './shop.component.scss',
})
export class ShopComponent implements OnInit {
  private shopService = inject(ShopService);
  private dialogService = inject(MatDialog);
  products: Product[] = [];
  selectedBrands: string[] = [];
  selectedTypes: string[] = [];
  selectedSort: string = 'name';
  sortOptions = [
    {
      name: 'Alphabertical',
      value: 'name',
    },
    {
      name: 'Price: Low-High',
      value: 'priceAsc',
    },
    {
      name: 'Price: High-Low',
      value: 'priceDesc',
    },
  ];

  ngOnInit(): void {
    this.initializeShop();
  }
  initializeShop() {
    this.shopService.getBrands();
    this.shopService.getType();
    this.getProducts();
  }
  getProducts() {
    this.shopService
      .getProducts(this.selectedBrands, this.selectedTypes, this.selectedSort)
      .subscribe({
        next: (response) => (this.products = response.data),
        error: (err) => console.log(err),
      });
  }
  onSortChange(event: MatSelectionListChange) {
    const selectedOption = event.options[0];
    if (selectedOption) {
      this.selectedSort = selectedOption.value;
      console.log(this.selectedSort);
      this.getProducts();
    }
  }
  openFilterDialog() {
    const dialogRef = this.dialogService.open(FiltersDialogComponent, {
      minWidth: '500px',
      data: {
        selectedBrands: this.selectedBrands,
        selectedTypes: this.selectedTypes,
      },
    });
    dialogRef.afterClosed().subscribe({
      next: (result) => {
        console.log(result);
        this.selectedBrands = result.selectedBrands;
        this.selectedTypes = result.selectedTypes;
        //apply filter here
        this.shopService
          .getProducts(this.selectedBrands, this.selectedTypes)
          .subscribe({
            next: (res) => (this.products = res.data),
            error: (err) => console.log(err),
          });
      },
    });
  }
}
