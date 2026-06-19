import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MaintenanceRecord } from '../../../models/maintenance.model';
import { MaintenanceService } from '../../../services/maintenance.service';
import { MaintenanceFormDialogComponent } from '../maintenance-form-dialog/maintenance-form-dialog.component';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-maintenance-list',
  imports: [
    MatTableModule,
    MatButtonModule,
    MatIconModule,
    MatTooltipModule,
    MatDialogModule,
    MatSnackBarModule,
    DatePipe,
    CurrencyPipe
  ],
  templateUrl: './maintenance-list.component.html',
  styleUrl: './maintenance-list.component.scss'
})
export class MaintenanceListComponent implements OnInit {
  private readonly maintenanceService = inject(MaintenanceService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly displayedColumns = ['tool', 'date', 'description', 'performedBy', 'cost', 'nextDate', 'actions'];
  readonly dataSource = new MatTableDataSource<MaintenanceRecord>([]);

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.maintenanceService.getAll().subscribe(data => {
      this.dataSource.data = data;
    });
  }

  openCreate(): void {
    this.dialog.open(MaintenanceFormDialogComponent, { width: '480px' })
      .afterClosed()
      .subscribe(result => this.refreshIfNeeded(result));
  }

  private refreshIfNeeded(result: unknown): void {
    if (result) {
      this.load();
    }
  }

  delete(record: MaintenanceRecord): void {
    this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete maintenance record',
        message: `Delete maintenance record for "${record.toolName}"?`,
        confirmText: 'Delete'
      }
    }).afterClosed().subscribe(confirmed => {
      if (!confirmed) {
        return;
      }

      this.maintenanceService.delete(record.id).subscribe({
        next: () => {
          this.snackBar.open('Record deleted', 'OK', { duration: 3000 });
          this.load();
        },
        error: () => this.snackBar.open('Failed to delete', 'OK', { duration: 3000 })
      });
    });
  }
}
