import { Component, OnDestroy, inject, signal } from '@angular/core';
import { BarcodeFormat } from '@zxing/library';
import { ZXingScannerModule } from '@zxing/ngx-scanner';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Tool } from '../../models/tool.model';
import { CheckoutService } from '../../services/checkout.service';
import { ToolService } from '../../services/tool.service';
import { getToolStatusColor, ToolStatusColor } from '../tool-status.util';

@Component({
  selector: 'app-scanner-dialog',
  standalone: true,
  imports: [
    ZXingScannerModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCardModule,
    MatChipsModule
  ],
  templateUrl: './scanner-dialog.component.html',
  styleUrl: './scanner-dialog.component.scss'
})
export class ScannerDialogComponent implements OnDestroy {
  private readonly toolService = inject(ToolService);
  private readonly checkoutService = inject(CheckoutService);
  private readonly dialogRef = inject(MatDialogRef<ScannerDialogComponent>);
  private readonly snackBar = inject(MatSnackBar);

  readonly formats = [
    BarcodeFormat.QR_CODE,
    BarcodeFormat.CODE_128,
    BarcodeFormat.CODE_39,
    BarcodeFormat.EAN_13,
    BarcodeFormat.EAN_8,
    BarcodeFormat.UPC_A,
    BarcodeFormat.UPC_E,
    BarcodeFormat.DATA_MATRIX
  ];

  readonly scanning = signal(true);
  readonly loading = signal(false);
  readonly foundTool = signal<Tool | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly lastScannedCode = signal<string | null>(null);

  onScanSuccess(code: string): void {
    if (this.loading() || code === this.lastScannedCode()) {
      return;
    }

    const toolId = this.extractToolId(code);
    this.lastScannedCode.set(code);
    this.scanning.set(false);
    this.loading.set(true);
    this.foundTool.set(null);
    this.errorMessage.set(null);

    this.toolService.getByBarcode(toolId).subscribe({
      next: tool => {
        this.loading.set(false);
        this.foundTool.set(tool);
      },
      error: err => {
        this.loading.set(false);
        this.errorMessage.set(
          err.status === 404
            ? `No tool found for barcode: ${toolId}`
            : 'Error looking up barcode. Please try again.'
        );
      }
    });
  }

  scanAgain(): void {
    this.scanning.set(true);
    this.loading.set(false);
    this.foundTool.set(null);
    this.errorMessage.set(null);
    this.lastScannedCode.set(null);
  }

  checkOut(tool: Tool): void {
    this.dialogRef.close({ action: 'checkout', tool });
  }

  checkIn(tool: Tool): void {
    this.checkoutService.checkInByToolId(tool.id).subscribe({
      next: () => {
        this.snackBar.open(`"${tool.name}" checked in successfully`, 'OK', { duration: 3000 });
        this.dialogRef.close({ action: 'checkin' });
      },
      error: err => {
        if (err.status === 404) {
          this.snackBar.open('No active checkout found for this tool', 'OK', { duration: 3000 });
          return;
        }

        this.snackBar.open('Failed to check in tool', 'OK', { duration: 3000 });
      }
    });
  }

  statusColor(status: Tool['status']): ToolStatusColor {
    return getToolStatusColor(status);
  }

  close(): void {
    this.dialogRef.close(null);
  }

  ngOnDestroy(): void {
    this.scanning.set(false);
  }

  private extractToolId(code: string): string {
    const scanned = code.trim();
    if (!scanned.startsWith('{')) {
      return scanned;
    }

    try {
      const parsed = JSON.parse(scanned) as { id?: unknown };
      if (typeof parsed.id === 'string' && parsed.id.trim()) {
        return parsed.id.trim();
      }
    } catch {
      // Keep backwards compatibility with plain barcode values.
    }

    return scanned;
  }
}
