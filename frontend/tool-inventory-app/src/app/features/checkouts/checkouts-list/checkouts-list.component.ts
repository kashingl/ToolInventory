import { DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Checkout } from '../../../models/checkout.model';
import { CheckoutService } from '../../../services/checkout.service';
import { CheckoutDialogComponent } from '../checkout-dialog/checkout-dialog.component';

@Component({
  selector: 'app-checkouts-list',
  imports: [
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatDialogModule,
    MatSnackBarModule,
    DatePipe
  ],
  templateUrl: './checkouts-list.component.html',
  styleUrl: './checkouts-list.component.scss'
})
export class CheckoutsListComponent implements OnInit {
  private readonly checkoutService = inject(CheckoutService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly displayedColumns = ['tool', 'user', 'checkoutDate', 'expectedReturn', 'status', 'actions'];
  readonly dataSource = new MatTableDataSource<Checkout>([]);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.checkoutService.getAll().subscribe(data => {
      this.dataSource.data = data;
    });
  }

  isActive(checkout: Checkout): boolean {
    return !checkout.actualReturnDate;
  }

  checkIn(checkout: Checkout): void {
    this.checkoutService.checkIn(checkout.id).subscribe({
      next: () => {
        this.snackBar.open('Tool checked in', 'OK', { duration: 3000 });
        this.load();
      },
      error: () => this.snackBar.open('Failed to check in', 'OK', { duration: 3000 })
    });
  }

  openCheckout(): void {
    this.dialog.open(CheckoutDialogComponent, { width: '480px' })
      .afterClosed()
      .subscribe(result => this.refreshIfNeeded(result));
  }

  private refreshIfNeeded(result: unknown): void {
    if (result) {
      this.load();
    }
  }
}
