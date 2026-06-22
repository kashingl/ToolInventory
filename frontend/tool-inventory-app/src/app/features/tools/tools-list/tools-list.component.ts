import { AfterViewInit, Component, OnInit, ViewChild, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatDividerModule } from '@angular/material/divider';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatPaginator, MatPaginatorModule } from '@angular/material/paginator';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { MatTooltipModule } from '@angular/material/tooltip';
import { Checkout } from '../../../models/checkout.model';
import { MaintenanceRecord } from '../../../models/maintenance.model';
import { Tool } from '../../../models/tool.model';
import { AuthService } from '../../../services/auth.service';
import { CheckoutService } from '../../../services/checkout.service';
import { MaintenanceService } from '../../../services/maintenance.service';
import { ToolService } from '../../../services/tool.service';
import { ConfirmDialogComponent } from '../../../shared/confirm-dialog/confirm-dialog.component';
import { ScannerDialogComponent } from '../../../shared/scanner-dialog/scanner-dialog.component';
import { getToolStatusColor, ToolStatusColor } from '../../../shared/tool-status.util';
import { ToolFormDialogComponent } from '../tool-form-dialog/tool-form-dialog.component';

type StatusFilter = 'all' | Tool['status'];
type QuickActionMode = 'checkout' | 'checkin' | null;

interface MetricCard {
  testId: string;
  label: string;
  value: number;
}

interface DashboardAlert {
  title: string;
  detail: string;
  route: '/checkouts' | '/maintenance';
  actionLabel: string;
}

@Component({
  selector: 'app-tools-list',
  imports: [
    ReactiveFormsModule,
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
    MatSnackBarModule,
    MatCardModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule
  ],
  templateUrl: './tools-list.component.html',
  styleUrl: './tools-list.component.scss'
})
export class ToolsListComponent implements OnInit, AfterViewInit {
  private readonly fb = inject(FormBuilder);
  private readonly toolService = inject(ToolService);
  private readonly checkoutService = inject(CheckoutService);
  private readonly maintenanceService = inject(MaintenanceService);
  private readonly dialog = inject(MatDialog);
  private readonly snackBar = inject(MatSnackBar);
  private readonly router = inject(Router);
  private readonly authService = inject(AuthService);

  readonly displayedColumns = ['tool', 'category', 'status', 'location', 'action', 'manage'];
  readonly dataSource = new MatTableDataSource<Tool>([]);
  readonly statusOptions: Array<{ value: StatusFilter; label: string }> = [
    { value: 'all', label: 'All statuses' },
    { value: 'Available', label: 'Available' },
    { value: 'CheckedOut', label: 'Checked out' },
    { value: 'UnderMaintenance', label: 'Under maintenance' },
    { value: 'Retired', label: 'Retired' }
  ];
  readonly quickActionForm = this.fb.group({
    userId: ['', [Validators.required, Validators.maxLength(450)]],
    expectedReturnDate: [null as Date | null],
    notes: ['']
  });

  allTools: Tool[] = [];
  allCheckouts: Checkout[] = [];
  allMaintenanceRecords: MaintenanceRecord[] = [];
  dashboardMetrics: MetricCard[] = [];
  alerts: DashboardAlert[] = [];
  selectedTool: Tool | null = null;
  quickActionMode: QuickActionMode = null;
  searchTerm = '';
  selectedStatus: StatusFilter = 'all';
  loadingDashboard = true;
  submittingQuickAction = false;

  @ViewChild(MatPaginator) paginator?: MatPaginator;
  @ViewChild(MatSort) sort?: MatSort;

  ngOnInit(): void {
    const currentUser = this.authService.currentUser();
    if (currentUser?.email) {
      this.quickActionForm.patchValue({ userId: currentUser.email });
    }

    this.load(true);
  }

  ngAfterViewInit(): void {
    if (this.paginator) {
      this.dataSource.paginator = this.paginator;
    }

    if (this.sort) {
      this.dataSource.sort = this.sort;
      this.dataSource.sortingDataAccessor = (tool, property) => {
        switch (property) {
          case 'tool':
            return tool.name;
          case 'category':
            return tool.categoryName ?? '';
          case 'status':
            return tool.status;
          case 'location':
            return `${tool.location ?? ''} ${tool.systainer ?? ''}`.trim();
          default:
            return '';
        }
      };
    }
  }

  load(resetPaginator = false): void {
    this.loadingDashboard = true;

    forkJoin({
      tools: this.toolService.getAll(),
      checkouts: this.checkoutService.getAll(),
      maintenance: this.maintenanceService.getAll()
    }).subscribe({
      next: ({ tools, checkouts, maintenance }) => {
        this.allTools = tools;
        this.allCheckouts = checkouts;
        this.allMaintenanceRecords = maintenance;
        this.rebuildDashboard();
        this.applyFilters(resetPaginator);
        this.syncSelectedTool();
        this.loadingDashboard = false;
      },
      error: () => {
        this.loadingDashboard = false;
        this.snackBar.open('Failed to load tools dashboard', 'OK', { duration: 3000 });
      }
    });
  }

  applySearch(event: Event): void {
    this.searchTerm = (event.target as HTMLInputElement).value;
    this.applyFilters(true);
  }

  changeStatus(status: StatusFilter): void {
    this.selectedStatus = status;
    this.applyFilters(true);
  }

  resetFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = 'all';
    this.applyFilters(true);
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

        if (result.action === 'checkout' && result.tool) {
          this.prepareQuickAction(result.tool, 'checkout');
          return;
        }

        if (result.action === 'checkin') {
          this.clearQuickAction();
          this.load();
        }
      });
  }

  handlePrimaryAction(tool: Tool): void {
    const mode = this.resolveQuickActionMode(tool);
    if (mode) {
      this.prepareQuickAction(tool, mode);
      return;
    }

    this.openEdit(tool);
  }

  prepareQuickAction(tool: Tool, mode: QuickActionMode = this.resolveQuickActionMode(tool)): void {
    this.selectedTool = tool;
    this.quickActionMode = mode;
    this.quickActionForm.patchValue({
      expectedReturnDate: null,
      notes: ''
    });
  }

  focusTool(tool: Tool): void {
    const mode = this.resolveQuickActionMode(tool);
    if (mode) {
      this.prepareQuickAction(tool, mode);
      return;
    }

    this.selectedTool = tool;
    this.quickActionMode = null;
  }

  clearQuickAction(): void {
    this.selectedTool = null;
    this.quickActionMode = null;
    this.quickActionForm.patchValue({
      expectedReturnDate: null,
      notes: ''
    });
  }

  submitQuickAction(): void {
    if (!this.selectedTool || !this.quickActionMode || this.submittingQuickAction) {
      return;
    }

    if (this.quickActionMode === 'checkin') {
      this.performCheckIn(this.selectedTool);
      return;
    }

    if (this.quickActionForm.invalid) {
      this.quickActionForm.markAllAsTouched();
      return;
    }

    const value = this.quickActionForm.getRawValue();
    const userId = (value.userId ?? '').trim();
    if (!userId) {
      this.quickActionForm.controls.userId.setErrors({ required: true });
      return;
    }

    this.submittingQuickAction = true;
    this.checkoutService.create({
      toolId: this.selectedTool.id,
      userId,
      expectedReturnDate: value.expectedReturnDate?.toISOString(),
      notes: value.notes?.trim() || undefined
    }).subscribe({
      next: () => {
        this.submittingQuickAction = false;
        this.snackBar.open(`"${this.selectedTool?.name}" checked out`, 'OK', { duration: 3000 });
        this.clearQuickAction();
        this.load();
      },
      error: err => {
        this.submittingQuickAction = false;
        this.snackBar.open(err?.error?.detail ?? 'Failed to check out tool', 'OK', { duration: 3000 });
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
          if (this.selectedTool?.id === tool.id) {
            this.clearQuickAction();
          }
          this.load();
        },
        error: () => this.snackBar.open('Failed to delete tool', 'OK', { duration: 3000 })
      });
    });
  }

  navigateTo(route: '/checkouts' | '/maintenance'): void {
    this.router.navigate([route]);
  }

  statusColor(status: Tool['status']): ToolStatusColor {
    return getToolStatusColor(status);
  }

  statusLabel(status: Tool['status']): string {
    return status === 'CheckedOut'
      ? 'Checked out'
      : status === 'UnderMaintenance'
        ? 'Under maintenance'
        : status;
  }

  primaryActionLabel(tool: Tool): string {
    switch (tool.status) {
      case 'Available':
        return 'Check Out';
      case 'CheckedOut':
        return 'Check In';
      case 'UnderMaintenance':
        return 'View';
      case 'Retired':
        return 'Edit';
    }
  }

  quickActionTitle(): string {
    if (!this.selectedTool || !this.quickActionMode) {
      return 'Select or scan a tool';
    }

    return this.quickActionMode === 'checkout'
      ? `Ready to check out ${this.selectedTool.name}`
      : `Ready to check in ${this.selectedTool.name}`;
  }

  quickActionDescription(): string {
    if (!this.selectedTool || !this.quickActionMode) {
      return 'Use the scanner or choose a row action to prepare a quick transaction.';
    }

    return this.quickActionMode === 'checkout'
      ? 'Verify the employee and confirm the checkout without leaving the dashboard.'
      : 'Confirm the return to set the tool back to Available immediately.';
  }

  toolMeta(tool: Tool): string {
    const modelInfo = [tool.make, tool.model].filter(Boolean).join(' ');
    return [modelInfo, tool.serialNumber, tool.owner, tool.systainer, tool.barcode].filter(Boolean).join(' | ')
      || 'No make, model, serial number, owner, barcode, or systainer assigned';
  }

  locationMeta(tool: Tool): string {
    return [tool.location, tool.description].filter(Boolean).join(' � ') || 'No location details';
  }

  private performCheckIn(tool: Tool): void {
    this.submittingQuickAction = true;
    this.checkoutService.checkInByToolId(tool.id).subscribe({
      next: () => {
        this.submittingQuickAction = false;
        this.snackBar.open(`"${tool.name}" checked in`, 'OK', { duration: 3000 });
        this.clearQuickAction();
        this.load();
      },
      error: err => {
        this.submittingQuickAction = false;
        this.snackBar.open(err?.error?.detail ?? 'Failed to check in tool', 'OK', { duration: 3000 });
      }
    });
  }

  private resolveQuickActionMode(tool: Tool): QuickActionMode {
    return tool.status === 'Available'
      ? 'checkout'
      : tool.status === 'CheckedOut'
        ? 'checkin'
        : null;
  }

  private applyFilters(resetPaginator = false): void {
    const query = this.searchTerm.trim().toLowerCase();
    const filtered = this.allTools.filter(tool => {
      const matchesStatus = this.selectedStatus === 'all' || tool.status === this.selectedStatus;
      if (!matchesStatus) {
        return false;
      }

      if (!query) {
        return true;
      }

      return [
        tool.name,
        tool.make,
        tool.model,
        tool.serialNumber,
        tool.owner,
        tool.categoryName,
        tool.location,
        tool.systainer,
        tool.barcode,
        tool.description,
        tool.status
      ].some(value => value?.toLowerCase().includes(query));
    });

    this.dataSource.data = filtered;
    if (resetPaginator && this.paginator) {
      this.paginator.firstPage();
    }
  }

  private rebuildDashboard(): void {
    const activeCheckouts = this.allCheckouts.filter(checkout => !checkout.actualReturnDate);
    const dueMaintenance = this.allMaintenanceRecords
      .filter(record => record.nextMaintenanceDate)
      .filter(record => {
        const nextDate = new Date(record.nextMaintenanceDate!);
        const daysUntil = Math.ceil((nextDate.getTime() - Date.now()) / 86400000);
        return daysUntil >= 0 && daysUntil <= 3;
      })
      .slice(0, 2);
    const overdueCheckouts = activeCheckouts
      .filter(checkout => Math.floor((Date.now() - new Date(checkout.checkoutDate).getTime()) / 86400000) >= 7)
      .slice(0, 2);

    this.dashboardMetrics = [
      {
        testId: 'metric-checkouts-today',
        label: 'Checked out today',
        value: this.allCheckouts.filter(checkout => this.isSameDay(checkout.checkoutDate, new Date())).length
      },
      {
        testId: 'metric-awaiting-return',
        label: 'Awaiting check-in',
        value: activeCheckouts.length
      },
      {
        testId: 'metric-maintenance',
        label: 'In maintenance',
        value: this.allTools.filter(tool => tool.status === 'UnderMaintenance').length
      },
      {
        testId: 'metric-available',
        label: 'Available now',
        value: this.allTools.filter(tool => tool.status === 'Available').length
      }
    ];

    this.alerts = [
      ...dueMaintenance.map(record => ({
        title: `${record.toolName} needs attention`,
        detail: `Next maintenance is scheduled for ${new Date(record.nextMaintenanceDate!).toLocaleDateString()}.`,
        route: '/maintenance' as const,
        actionLabel: 'Review maintenance'
      })),
      ...overdueCheckouts.map(checkout => ({
        title: `${checkout.toolName} is overdue`,
        detail: `Checked out to ${checkout.userName || checkout.userId} on ${new Date(checkout.checkoutDate).toLocaleDateString()}.`,
        route: '/checkouts' as const,
        actionLabel: 'Open checkouts'
      }))
    ].slice(0, 4);
  }

  private syncSelectedTool(): void {
    if (!this.selectedTool) {
      return;
    }

    const refreshed = this.allTools.find(tool => tool.id === this.selectedTool?.id);
    if (!refreshed) {
      this.clearQuickAction();
      return;
    }

    this.selectedTool = refreshed;
    if (!this.resolveQuickActionMode(refreshed)) {
      this.quickActionMode = null;
    }
  }

  private refreshIfNeeded(result: unknown): void {
    if (result) {
      this.load();
    }
  }

  private isSameDay(value: string, compareDate: Date): boolean {
    const date = new Date(value);
    return date.getFullYear() === compareDate.getFullYear()
      && date.getMonth() === compareDate.getMonth()
      && date.getDate() === compareDate.getDate();
  }
}
