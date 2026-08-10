import { generateColors } from '@mantine/colors-generator'
import {
  ActionIcon,
  Avatar,
  Badge,
  Button,
  Card,
  Checkbox,
  CloseButton,
  Code,
  Drawer,
  Input,
  InputWrapper,
  Loader,
  LoadingOverlay,
  MantineThemeOverride,
  Menu,
  Modal,
  Notification,
  Pagination,
  Paper,
  Popover,
  Radio,
  SegmentedControl,
  Skeleton,
  Switch,
  Table,
  Tabs,
  Title,
  Tooltip,
  TooltipFloating,
  createTheme,
  useMantineTheme,
} from '@mantine/core'
import { createStyles } from '@mantine/emotion'
import { useLocalStorage, useMediaQuery } from '@mantine/hooks'
import { useEffect, useState } from 'react'
import { useConfig } from '@Hooks/useConfig'
import designSystemClasses from '@Styles/DesignSystem.module.css'

const CustomTheme: MantineThemeOverride = {
  colors: {
    gray: [
      '#F6F5EF',
      '#E7E7E3',
      '#D5D4CF',
      '#BDBCB6',
      '#9C9B95',
      '#7A7974',
      '#5B5A56',
      '#3B3A37',
      '#292826',
      '#191919',
    ],
    brand: [
      '#FFF0F2',
      '#FFDDE2',
      '#FFB7C1',
      '#FF8999',
      '#EF4D62',
      '#DD1835',
      '#C9001B',
      '#B50018',
      '#8E0013',
      '#65000D',
    ],
    coral: [
      '#FFF0F2',
      '#FFDDE2',
      '#FFB7C1',
      '#FF8999',
      '#EF4D62',
      '#DD1835',
      '#C9001B',
      '#B50018',
      '#8E0013',
      '#65000D',
    ],
    sun: ['#FFFBE2', '#FFF6B8', '#FFEC74', '#FFE33D', '#FFD91A', '#EFC600', '#C9A500', '#9C7F00', '#715C00', '#4B3D00'],
    alert: [
      '#FFB4B4',
      '#FFA0A0',
      '#FF8c8c',
      '#FF7878',
      '#FF6464',
      '#FE5050',
      '#FE3c3c',
      '#FE2828',
      '#FC1414',
      '#FC0000',
    ],
    light: [
      '#FFFFFF',
      '#F8F8F8',
      '#EFEFEF',
      '#E0E0E0',
      '#DFDFDF',
      '#D0D0D0',
      '#CFCFCF',
      '#C0C0C0',
      '#BFBFBF',
      '#B0B0B0',
    ],
    dark: [
      '#F7F7F4',
      '#E0E3DE',
      '#B8BFBA',
      '#8D9690',
      '#666E69',
      '#464D49',
      '#303532',
      '#222624',
      '#171A18',
      '#0F1110',
    ],
  },
  primaryColor: 'brand',
  primaryShade: { light: 6, dark: 5 },
  autoContrast: true,
  fontFamily:
    'Lexend, -apple-system, BlinkMacSystemFont, Helvetica Neue, PingFang SC, Microsoft YaHei, Source Han Sans SC, Noto Sans CJK SC, sans-serif',
  fontFamilyMonospace:
    'JetBrains Mono, ui-monospace, SFMono-Regular, Monaco, Consolas, Courier New, monospace, sans-serif',
  headings: {
    fontFamily: 'Anybody, Lexend, sans-serif',
  },
  defaultRadius: 'sm',
  radius: {
    xs: '0',
    sm: '2px',
    md: '4px',
    lg: '6px',
    xl: '8px',
  },
  shadows: {
    xs: '1px 1px 0 #000000',
    sm: '2px 2px 0 #000000',
    md: '3px 3px 0 #000000',
    lg: '4px 4px 0 #000000',
    xl: '5px 5px 0 #000000',
  },
  breakpoints: {
    xs: '30em',
    sm: '48em',
    md: '64em',
    lg: '74em',
    xl: '90em',
    w18: '1800px',
    w24: '2400px',
    w30: '3000px',
    w36: '3600px',
    w42: '4200px',
    w48: '4800px',
  },
  components: {
    Title: Title.extend({
      classNames: { root: designSystemClasses.title },
    }),
    Button: Button.extend({
      classNames: {
        root: designSystemClasses.button,
        label: designSystemClasses.controlLabel,
      },
    }),
    ActionIcon: ActionIcon.extend({
      defaultProps: {
        variant: 'transparent',
      },
      classNames: { root: designSystemClasses.actionIcon },
    }),
    CloseButton: CloseButton.extend({
      classNames: { root: designSystemClasses.closeButton },
    }),
    Input: Input.extend({
      classNames: {
        input: designSystemClasses.input,
        section: designSystemClasses.inputSection,
      },
    }),
    InputWrapper: InputWrapper.extend({
      classNames: {
        label: designSystemClasses.inputLabel,
        description: designSystemClasses.inputDescription,
        error: designSystemClasses.inputError,
      },
    }),
    Card: Card.extend({
      classNames: { root: designSystemClasses.surface },
    }),
    Paper: Paper.extend({
      classNames: { root: designSystemClasses.surface },
    }),
    Loader: Loader.extend({
      defaultProps: {
        type: 'bars',
      },
      classNames: { root: designSystemClasses.loader },
    }),
    LoadingOverlay: LoadingOverlay.extend({
      defaultProps: {
        overlayProps: { backgroundOpacity: 0.72, blur: 0 },
      },
      classNames: {
        overlay: designSystemClasses.loadingOverlay,
        loader: designSystemClasses.loadingOverlayLoader,
      },
    }),
    Skeleton: Skeleton.extend({
      classNames: { root: designSystemClasses.skeleton },
    }),
    Switch: Switch.extend({
      classNames: {
        root: designSystemClasses.toggleRoot,
        body: designSystemClasses.toggleBody,
        labelWrapper: designSystemClasses.toggleLabelWrapper,
        track: designSystemClasses.switchTrack,
        thumb: designSystemClasses.switchThumb,
        input: designSystemClasses.toggleInput,
        label: designSystemClasses.toggleLabel,
      },
    }),
    Checkbox: Checkbox.extend({
      classNames: {
        root: designSystemClasses.toggleRoot,
        input: designSystemClasses.checkboxInput,
        icon: designSystemClasses.checkboxIcon,
        label: designSystemClasses.toggleLabel,
      },
    }),
    Radio: Radio.extend({
      classNames: {
        root: designSystemClasses.toggleRoot,
        radio: designSystemClasses.radioInput,
        icon: designSystemClasses.radioIcon,
        label: designSystemClasses.toggleLabel,
      },
    }),
    Modal: Modal.extend({
      defaultProps: {
        centered: true,
        transitionProps: { duration: 140, transition: 'fade-down' },
      },
      classNames: {
        content: designSystemClasses.overlaySurface,
        header: designSystemClasses.overlayHeader,
        title: designSystemClasses.overlayTitle,
        body: designSystemClasses.overlayBody,
        overlay: designSystemClasses.overlay,
        close: designSystemClasses.closeButton,
      },
    }),
    Drawer: Drawer.extend({
      defaultProps: {
        transitionProps: { duration: 140, transition: 'slide-left' },
      },
      classNames: {
        content: designSystemClasses.overlaySurface,
        header: designSystemClasses.overlayHeader,
        title: designSystemClasses.overlayTitle,
        body: designSystemClasses.overlayBody,
        overlay: designSystemClasses.overlay,
        close: designSystemClasses.closeButton,
      },
    }),
    Popover: Popover.extend({
      defaultProps: {
        withinPortal: true,
      },
      classNames: {
        dropdown: designSystemClasses.floatingSurface,
        arrow: designSystemClasses.floatingArrow,
      },
    }),
    Badge: Badge.extend({
      defaultProps: {
        variant: 'outline',
      },
      classNames: {
        root: designSystemClasses.badge,
        label: designSystemClasses.badgeLabel,
      },
    }),
    Tabs: Tabs.extend({
      classNames: {
        list: designSystemClasses.tabsList,
        tab: designSystemClasses.tab,
        tabLabel: designSystemClasses.controlLabel,
      },
    }),
    SegmentedControl: SegmentedControl.extend({
      defaultProps: {
        transitionDuration: 120,
      },
      classNames: {
        root: designSystemClasses.segmentedControl,
        control: designSystemClasses.segmentControl,
        indicator: designSystemClasses.segmentIndicator,
        label: designSystemClasses.segmentLabel,
      },
    }),
    Table: Table.extend({
      defaultProps: {
        highlightOnHover: true,
        horizontalSpacing: 'xs',
        verticalSpacing: 'xs',
      },
      classNames: {
        table: designSystemClasses.table,
        thead: designSystemClasses.tableHead,
        tr: designSystemClasses.tableRow,
        th: designSystemClasses.tableHeaderCell,
        td: designSystemClasses.tableCell,
        caption: designSystemClasses.tableCaption,
      },
    }),
    Pagination: Pagination.extend({
      classNames: {
        control: designSystemClasses.paginationControl,
        dots: designSystemClasses.paginationDots,
      },
    }),
    Notification: Notification.extend({
      classNames: {
        root: designSystemClasses.notification,
        icon: designSystemClasses.notificationIcon,
        title: designSystemClasses.notificationTitle,
        description: designSystemClasses.notificationDescription,
        closeButton: designSystemClasses.closeButton,
      },
    }),
    Avatar: Avatar.extend({
      defaultProps: {
        color: 'brand',
      },
    }),
    Menu: Menu.extend({
      classNames: {
        dropdown: designSystemClasses.floatingSurface,
        item: designSystemClasses.menuItem,
        label: designSystemClasses.menuLabel,
        divider: designSystemClasses.menuDivider,
      },
    }),
    Code: Code.extend({
      styles: {
        root: {
          fontWeight: 500,
        },
      },
    }),
    Tooltip: Tooltip.extend({
      classNames: {
        tooltip: designSystemClasses.tooltip,
        arrow: designSystemClasses.tooltipArrow,
      },
    }),
    TooltipFloating: TooltipFloating.extend({
      classNames: {
        tooltip: designSystemClasses.tooltip,
        arrow: designSystemClasses.tooltipArrow,
      },
    }),
  },
}

export enum ColorProvider {
  Managed = 'Managed',
  Default = 'Default',
  Custom = 'Custom',
}

export interface CustomColor {
  provider: ColorProvider
  color: string
}

export const useCustomColor = () => {
  const [customColor, setCustomColorInner] = useLocalStorage<CustomColor>({
    key: 'custom-theme',
    defaultValue: { provider: ColorProvider.Managed, color: '' } as CustomColor,
    getInitialValueInEffect: false,
    serialize: (value: CustomColor) => {
      if (value.provider === ColorProvider.Custom && /^#[0-9A-F]{6}$/i.test(value.color)) {
        return value.color
      } else if (value.provider === ColorProvider.Managed) {
        return ''
      } else {
        return 'brand'
      }
    },
    deserialize: (value?: string) => {
      if (typeof value !== 'string') return { provider: ColorProvider.Managed, color: '' }

      if (value === 'brand') {
        return { provider: ColorProvider.Default, color: '' }
      } else if (/^#[0-9A-F]{6}$/i.test(value)) {
        return { provider: ColorProvider.Custom, color: value }
      } else {
        return { provider: ColorProvider.Managed, color: '' }
      }
    },
  })

  const setCustomColor = (color: CustomColor) => {
    // validate custom color, do not save invalid values
    if (color.provider === ColorProvider.Custom && !/^#[0-9A-F]{6}$/i.test(color.color)) return

    setCustomColorInner(color)
  }

  // color: null for use platform color, 'brand' for default theme
  //        or hex color string for custom color
  return { customColor, setCustomColor }
}

export const useCustomTheme = () => {
  const { config } = useConfig()
  const { customColor } = useCustomColor()

  const resolveManaged = (color: string | null | undefined) => {
    return color && /^#[0-9A-F]{6}$/i.test(color) ? color : null
  }

  const [theme, setTheme] = useState<MantineThemeOverride>(createTheme(CustomTheme))

  useEffect(() => {
    if (customColor.provider === ColorProvider.Default) {
      setTheme(CustomTheme)
      return
    }

    const resolvedColor =
      customColor.provider === ColorProvider.Custom
        ? customColor.color
        : customColor.provider === ColorProvider.Managed
          ? resolveManaged(config.customTheme)
          : null

    if (resolvedColor) {
      setTheme({
        ...CustomTheme,
        colors: {
          ...CustomTheme.colors,
          custom: generateColors(resolvedColor),
        },
        components: {
          ...CustomTheme.components,
          Avatar: Avatar.extend({
            defaultProps: {
              color: 'custom',
            },
          }),
        },
        primaryColor: 'custom',
      })
    } else {
      setTheme(CustomTheme)
    }
  }, [customColor, config.customTheme])

  return { theme }
}

export const useIsMobile = (limit?: number) => {
  const theme = useMantineTheme()
  const isMobile = useMediaQuery(`(max-width: ${limit ? `${limit}px` : theme.breakpoints.sm})`)
  return isMobile
}

interface UseDisplayInputStylesProps {
  ff?: 'monospace' | 'text'
  fw?: React.CSSProperties['fontWeight']
  lh?: React.CSSProperties['lineHeight']
  cs?: React.CSSProperties['cursor']
}

export const useDisplayInputStyles = createStyles(
  (theme, { fw = 'normal', lh = '1.5rem', ff = 'text', cs = 'auto' }: UseDisplayInputStylesProps) => ({
    wrapper: {
      width: '100%',
    },
    input: {
      fontWeight: fw,
      fontFamily: ff === 'text' ? theme.fontFamily : theme.fontFamilyMonospace,
      height: lh,
      lineHeight: lh,
      cursor: cs,
      userSelect: 'none',
      minHeight: '1rem',
      maxHeight: '2rem',
    },
  })
)
