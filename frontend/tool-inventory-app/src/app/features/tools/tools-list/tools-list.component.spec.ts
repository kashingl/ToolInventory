import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { DomSanitizer } from '@angular/platform-browser';
import { provideRouter } from '@angular/router';
import { MatIconRegistry } from '@angular/material/icon';
import { of } from 'rxjs';
import { AuthService } from '../../../services/auth.service';
import { CheckoutService } from '../../../services/checkout.service';
import { MaintenanceService } from '../../../services/maintenance.service';
import { ToolService } from '../../../services/tool.service';
import { ToolsListComponent } from './tools-list.component';

const tools = [
  {
    id: 1,
    name: 'Bosch Drill',
    categoryName: 'Power tools',
    location: 'Warehouse A',
    systainer: 'SYS-100',
    barcode: 'DRILL-100',
    description: 'Cordless drill',
    status: 'Available' as const
  },
  {
    id: 2,
    name: 'Makita Grinder',
    categoryName: 'Power tools',
    location: 'Project Delta',
    systainer: 'SYS-200',
    barcode: 'GRINDER-200',
    description: 'Angle grinder',
    status: 'CheckedOut' as const
  },
  {
    id: 3,
    name: 'Torque Wrench',
    categoryName: 'Hand tools',
    location: 'Bench 3',
    systainer: 'SYS-300',
    barcode: 'TORQUE-300',
    description: 'Needs calibration',
    status: 'UnderMaintenance' as const
  }
];

const checkouts = [
  {
    id: 10,
    toolId: 2,
    toolName: 'Makita Grinder',
    userId: 'user-1',
    userName: 'Jane Doe',
    checkoutDate: new Date().toISOString(),
    expectedReturnDate: undefined,
    actualReturnDate: undefined,
    notes: 'Urgent project'
  }
];

const maintenance = [
  {
    id: 20,
    toolId: 3,
    toolName: 'Torque Wrench',
    date: new Date().toISOString(),
    description: 'Calibration',
    performedBy: 'Tech Ops',
    cost: 35,
    nextMaintenanceDate: new Date(Date.now() + 2 * 86400000).toISOString()
  }
];

const SVG_ICON = `<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24"><circle cx="12" cy="12" r="10"/></svg>`;

describe('ToolsListComponent', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ToolsListComponent, NoopAnimationsModule],
      providers: [
        provideHttpClient(),
        provideRouter([]),
        {
          provide: ToolService,
          useValue: {
            getAll: () => of(tools),
            delete: () => of(void 0)
          }
        },
        {
          provide: CheckoutService,
          useValue: {
            getAll: () => of(checkouts),
            create: () => of({}),
            checkInByToolId: () => of(void 0)
          }
        },
        {
          provide: MaintenanceService,
          useValue: {
            getAll: () => of(maintenance)
          }
        },
        {
          provide: AuthService,
          useValue: {
            currentUser: () => ({ email: 'worker@example.com' })
          }
        }
      ]
    }).compileComponents();

    const registry = TestBed.inject(MatIconRegistry);
    const sanitizer = TestBed.inject(DomSanitizer);
    for (const name of ['ui-search', 'ui-mail', 'ui-user', 'ui-qr']) {
      registry.addSvgIconLiteral(name, sanitizer.bypassSecurityTrustHtml(SVG_ICON));
    }
  });

  it('renders dashboard metrics for tools, checkouts, and maintenance', async () => {
    const fixture = TestBed.createComponent(ToolsListComponent);
    await fixture.whenStable();
    fixture.detectChanges();

    const element = fixture.nativeElement as HTMLElement;
    expect(element.querySelector('[data-testid="metric-checkouts-today"] .metric-card__value')?.textContent?.trim()).toBe('1');
    expect(element.querySelector('[data-testid="metric-awaiting-return"] .metric-card__value')?.textContent?.trim()).toBe('1');
    expect(element.querySelector('[data-testid="metric-maintenance"] .metric-card__value')?.textContent?.trim()).toBe('1');
    expect(element.querySelector('[data-testid="metric-available"] .metric-card__value')?.textContent?.trim()).toBe('1');
  });

  it('selects a tool into the quick action panel when the row action is clicked', async () => {
    const fixture = TestBed.createComponent(ToolsListComponent);
    await fixture.whenStable();
    fixture.detectChanges();

    const actionButton = fixture.nativeElement.querySelector('[data-testid="row-action-1"]') as HTMLButtonElement;
    actionButton.click();
    fixture.detectChanges();

    const quickPanelTitle = fixture.nativeElement.querySelector('[data-testid="quick-panel-selection"]') as HTMLElement;
    expect(quickPanelTitle.textContent).toContain('Ready to check out Bosch Drill');
    expect(fixture.nativeElement.textContent).toContain('Confirm Check Out');
  });
});
