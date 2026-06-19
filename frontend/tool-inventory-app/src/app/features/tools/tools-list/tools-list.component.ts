import { AfterViewInit, Component, OnInit, ViewChild, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Tool } from '../../../models/tool.model';
import { ToolService } from '../../../services/tool.service';
import { CheckoutDialogComponent } from '../../checkouts/checkout-dialog/checkout-dialog.component';
import { ScannerDialogComponent } from '../../../shared/scanner-dialog/scanner-dialog.component';
import { ToolFormDialogComponent } from '../tool-form-dialog/tool-form-dialog.component';
import { getToolStatusColor, ToolStatusColor } from '../../../shared/tool-status.util';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';

@Component({
  selector: 'app-tools-list',
  imports: [
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatInputModule,
    MatFormFieldModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatTooltipModule,
    MatDialogModule,
    MatDividerModule,
    MatSnackBarModule
  ],
  templateUrl: './tools-list.component.html',
  styleUrl: './tools-list.component.scss'
})
export class ToolsListComponent implements OnInit, AfterViewInit {
  private readonly toolService = inject(ToolService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);

  readonly displayedColumns = ['name', 'category', 'location', 'systainer', 'status', 'actions'];
  readonly dataSource = new MatTableDataSource<Tool>([]);

  @ViewChild(MatPaginator) paginator?: MatPaginator;
  @ViewChild(MatSort) sort?: MatSort;

  ngOnInit(): void {
    this.load();
  }

  ngAfterViewInit(): void {
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }

    if (this.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  load(): void {
    this.toolService.getAll().subscribe(tools => {
      this.dataSource.data = tools;
    });
  }

  applyFilter(event: Event): void {
    this.dataSource.filter = (event.target as HTMLInputElement).value.trim().toLowerCase();
  }

  openCreate(): void {
    this.dialog.open(ToolFormDialogComponent, { width: '500px' })
      .afterClosed()
      .subscribe(result => this.refreshIfNeeded(result));
  }

  openEdit(tool: Tool): void {
    this.dialog.open(ToolFormDialogComponent, { width: '500px', data: tool })
      .afterClosed()
      .subscribe(result => this.refreshIfNeeded(result));
  }

  openScanner(): void {
    this.dialog.open(ScannerDialogComponent, { width: '480px' })
      .afterClosed()
      .subscribe(result => {
        if (!result) {
          return;
        }

        if (result.action === 'checkout') {
          this.dialog.open(CheckoutDialogComponent, { width: '480px', data: result.tool })
            .afterClosed()
            .subscribe(checkoutResult => this.refreshIfNeeded(checkoutResult));
        } else if (result.action === 'checkin') {
          this.load();
        }
      });
  }

  delete(tool: Tool): void {
    this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data: {
        title: 'Delete tool',
        message: `Delete "${tool.name}"?`,
        confirmText: 'Delete'
      }
    }).afterClosed().subscribe(confirmed => {
      if (!confirmed) {
        return;
      }

      this.toolService.delete(tool.id).subscribe({
        next: () => {
          this.snackBar.open('Tool deleted', 'OK', { duration: 3000 });
          this.load();
        },
        error: () => this.snackBar.open('Failed to delete tool', 'OK', { duration: 3000 })
      });
    });
  }

  statusColor(status: Tool['status']): ToolStatusColor {
    return getToolStatusColor(status);
  }

  private refreshIfNeeded(result: unknown): void {
    if (result) {
      this.load();
    }
  }
}
